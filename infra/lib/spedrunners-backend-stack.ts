import * as cdk from 'aws-cdk-lib';
import { AttributeType, BillingMode, Table } from 'aws-cdk-lib/aws-dynamodb';
import { Construct } from 'constructs';
import { EnvironmentConfiguration } from './environment-configurations';
import { AccountRecovery, OAuthScope, ProviderAttribute, ResourceServerScope, UserPool, UserPoolClientIdentityProvider, UserPoolIdentityProviderGoogle } from 'aws-cdk-lib/aws-cognito';
import { Code, Function, Runtime } from 'aws-cdk-lib/aws-lambda';
import { CognitoUserPoolsAuthorizer, LambdaIntegration, RestApi } from 'aws-cdk-lib/aws-apigateway';
import { DnsValidatedCertificate } from 'aws-cdk-lib/aws-certificatemanager';
import { ARecord, HostedZone, RecordTarget } from 'aws-cdk-lib/aws-route53';
import { ApiGateway } from 'aws-cdk-lib/aws-route53-targets';
import { Policy, PolicyStatement } from 'aws-cdk-lib/aws-iam';

interface SpedrunnersBackendStackProps extends cdk.StackProps {
  envConfig: EnvironmentConfiguration
}

export class SpedrunnersBackendStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: SpedrunnersBackendStackProps) {
    super(scope, id, props);

    const envConfig = props.envConfig;
    const projectName = props.envConfig.projectName;
    const stage = props.envConfig.stage;

    const userPool = this.setupCognitoUserPool(projectName, props.envConfig);

    const table = new Table(this, `DynamoDBTable`, {
      tableName: `${projectName}-table-${this.region}-${stage}`,
      partitionKey: { name: 'PK', type: AttributeType.STRING },
      sortKey: { name: 'SK', type: AttributeType.STRING },
      billingMode: BillingMode.PAY_PER_REQUEST,
      removalPolicy: cdk.RemovalPolicy.DESTROY
    });

    const lambdaFunction = new Function(this, 'MoviesAPILambda', {
      functionName: `${projectName}-movies-api-${this.region}-${stage}`,
      runtime: Runtime.DOTNET_6,
      code: Code.fromAsset(`../SpedrunnersBackend/MoviesAPI/src/MoviesAPI/bin/build-package.zip`),
      handler: 'MoviesAPI',
      timeout: cdk.Duration.seconds(10),
      memorySize: 256,
      reservedConcurrentExecutions: 2,
      environment: {
        'TABLE_NAME': table.tableName,
        'USERINFO_ENDPOINT_URL': `https://${envConfig.cognitoHostedUiDomainPrefix}.auth.${this.region}.amazoncognito.com/oauth2/userInfo`
      }
    });
    table.grantReadWriteData(lambdaFunction);
    const lambdaIntegration = new LambdaIntegration(lambdaFunction);

    const hostedZone = HostedZone.fromLookup(this, 'Z05241663C6V8VT2JGS2K', {
      domainName: 'cloudchaotic.com'
    });
    const apiDomainName = `${envConfig.stageSubDomain}spedrunners.api.cloudchaotic.com`
    const certificate = new DnsValidatedCertificate(this, 'Certificate', {
      domainName: apiDomainName,
      hostedZone: hostedZone,
      cleanupRoute53Records: true // not recommended for production use
    });
    const api = new RestApi(this, 'Spedrunners API', {
      restApiName: `${projectName}-api-${this.region}-${stage}`,
      domainName: {
        domainName: apiDomainName,
        certificate: certificate
      }
    });
    const proxyResource = api.root.addResource('{proxy+}');
    proxyResource.addMethod('ANY', lambdaIntegration, {
      authorizer: new CognitoUserPoolsAuthorizer(this, 'CognitoAuthorizer', {
        cognitoUserPools: [userPool]
      }),
      authorizationScopes: [ `https://${apiDomainName}/*` ]
    });
    /*
    The admin proxy resource and cors preflight are set explicitly to avoid the cors OPTIONS method 
    using the Cognito Authorizer, which would cause the cors preflight check to fail. Curiously enough,
    cors also has to be set in the .NET application.
    */
    proxyResource.addCorsPreflight({
      allowOrigins: envConfig.origins
    });

    new ARecord(this, 'Spedrunners API ARecord', {
      zone: hostedZone,
      recordName: `${envConfig.stageSubDomain}spedrunners.api`,
      target: RecordTarget.fromAlias(new ApiGateway(api)),
      ttl: cdk.Duration.seconds(0)
    });
  }

  private setupCognitoUserPool(projectName: string, envConfig: EnvironmentConfiguration): UserPool {
    const stage = envConfig.stage;
    const preSignUpLambda = new Function(this, 'PreSignUpLambda', {
      functionName: `${projectName}-pre-sign-up-${this.region}-${stage}`,
      runtime: Runtime.PYTHON_3_11,
      code: Code.fromAsset('lib/pre-sign-up-function/build-package.zip'),
      handler: 'pre-sign-up.lambda_handler',
      timeout: cdk.Duration.seconds(10),
      memorySize: 128,
      reservedConcurrentExecutions: 1
    });
    const userPool = new UserPool(this, `${projectName}-user-pool-${this.region}-${stage}`, {
      selfSignUpEnabled: false,
      lambdaTriggers: {
        preSignUp: preSignUpLambda
      },
      userPoolName: `${projectName}-user-pool-${this.region}-${stage}`,
      signInAliases: { username: true, email: true },
      accountRecovery: AccountRecovery.EMAIL_ONLY,
      standardAttributes: {
        email: { required: true }
      },
      removalPolicy: cdk.RemovalPolicy.DESTROY
    });
    preSignUpLambda.role!.attachInlinePolicy(new Policy(this, 'PreSignUpFunctionPolicy', {
      statements: [ new PolicyStatement({
        actions: ['cognito-idp:ListUsers', 'cognito-idp:AdminLinkProviderForUser'],
        resources: [userPool.userPoolArn]
      })]
    }));
    userPool.addDomain('UserPoolDomain', {
      cognitoDomain: {
        domainPrefix: envConfig.cognitoHostedUiDomainPrefix
      }
    });

    const fullAccessScope = new ResourceServerScope({ scopeName: '*', scopeDescription: 'Full access' });
    const resourceServer = userPool.addResourceServer('ResourceServer', {
      userPoolResourceServerName: 'Spedrunners API',
      identifier: `https://${envConfig.stageSubDomain}spedrunners.api.cloudchaotic.com`,
      scopes: [fullAccessScope]
    });

    const client = userPool.addClient('spedrunners', {
      supportedIdentityProviders: [
        UserPoolClientIdentityProvider.GOOGLE
      ],
      oAuth: {
        flows: {
          authorizationCodeGrant: true
        },
        callbackUrls: envConfig.origins,
        logoutUrls: envConfig.origins,
        scopes: [
          OAuthScope.resourceServer(resourceServer, fullAccessScope),
          OAuthScope.PROFILE,
          OAuthScope.EMAIL
        ]
      }
    });

    const googleProvider = new UserPoolIdentityProviderGoogle(this, 'Google', {
      userPool: userPool,
      // client ID and secret are replaced in the console.
      clientId: 'REPLACE-ME',
      clientSecret: 'REPLACE-ME',
      scopes: ['profile', 'email'],
      attributeMapping: {
        email: ProviderAttribute.GOOGLE_EMAIL
      }
    });
    client.node.addDependency(googleProvider);

    return userPool;
  }
}
