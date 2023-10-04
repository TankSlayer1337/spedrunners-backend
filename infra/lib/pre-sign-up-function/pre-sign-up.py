# https://stackoverflow.com/a/69360336
import json
import boto3

client = boto3.client('cognito-idp')


def lambda_handler(event, context):
    print("Event: ", event)
    email = event['request']['userAttributes']['email']

    # Find a user with the same email
    response = client.list_users(
        UserPoolId=event['userPoolId'],
        AttributesToGet=[
            'email',
        ],
        Filter='email = \"{}\"'.format(email)
    )

    print("Reponse: ", response)

    if 'Users' not in response or not response['Users']:
        print("We ended up here...")
        return "You cannot Sign Up with this email"
    
    print('Users found: ', response['Users'])

    for user in response['Users']:
        provider = None
        provider_value = None
        # Check which provider it is using
        if event['userName'].startswith('google_'):
            provider = 'Google'
            provider_value = event['userName'].split('_')[1]

        print('Linking accounts from Email {} with provider {}: '.format(
            email,
            provider
        ))

        # If the signup is coming from a social provider, link the accounts
        # with admin_link_provider_for_user function
        if provider and provider_value:
            print('> Linking user: ', user)
            print('> Provider Id: ', provider_value)
            response = client.admin_link_provider_for_user(
                UserPoolId=event['userPoolId'],
                DestinationUser={
                    'ProviderName': 'Cognito',
                    'ProviderAttributeValue': user['Username']
                },
                SourceUser={
                    'ProviderName': provider,
                    'ProviderAttributeName': 'Cognito_Subject',
                    'ProviderAttributeValue': provider_value
                }
            )
    # Return the event to continue the workflow
    return event