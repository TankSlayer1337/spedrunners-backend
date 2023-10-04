# Welcome to your CDK TypeScript project

This is a blank project for CDK development with TypeScript.

The `cdk.json` file tells the CDK Toolkit how to execute your app.

## Useful commands

* `npm run build`   compile typescript to js
* `npm run watch`   watch for changes and compile
* `npm run test`    perform the jest unit tests
* `cdk deploy`      deploy this stack to your default AWS account/region
* `cdk diff`        compare deployed stack with current state
* `cdk synth`       emits the synthesized CloudFormation template

## Deploy spedrunners-backend stack
## Package API Lambda
Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

To build the Lambda package, move into the project folder and run:
```
    dotnet lambda package "bin/build-package.zip"
```

## Package Pre Sign-Up Lambda
1. Move into infra/lib/pre-sign-up-function.
2. Run
```
    Compress-Archive pre-sign-up.py build-package.zip
```

## Limit Google sign in
Did an ugly workaround to limit sign in to Google accounts of my choice. In order for it to work, the users has to be added through the console while the preSignUp trigger property for the user pool is undefined. After the users has been added, the trigger should be defined.