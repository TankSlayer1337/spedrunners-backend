import { Environment } from "aws-cdk-lib"

export interface EnvironmentConfiguration {
  awsEnv: Environment,
  projectName: string,
  stage: string,
  stageSubDomain: string,
  origins: string[],
  cognitoHostedUiDomainPrefix: string
}

const stockholm: Environment = { region: 'eu-north-1', account: process.env.CDK_DEFAULT_ACCOUNT };
const projectName = 'spedrunners-backend';

export const devConfiguration: EnvironmentConfiguration = {
  awsEnv: stockholm,
  projectName: projectName,
  stage: 'dev',
  stageSubDomain: 'dev.',
  origins: [
    'http://localhost:5173',
    'https://dev.spedrunners.cloudchaotic.com'
  ],
  cognitoHostedUiDomainPrefix: 'spedrunners-dev'
}

export const prodConfiguration: EnvironmentConfiguration = {
  awsEnv: stockholm,
  projectName: projectName,
  stage: 'prod',
  stageSubDomain: 'prod.',
  origins: [ 'https://spedrunners.cloudchaotic.com' ],
  cognitoHostedUiDomainPrefix: 'spedrunners-prod'
}

export const environmentConfigurations: EnvironmentConfiguration[] = [ devConfiguration, prodConfiguration ]