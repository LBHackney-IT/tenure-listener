#!/bin/bash

# Script to migrate environment variables from source account to disaster recovery account
# Usage: ./migrate-env-vars.sh <source-profile>

if [ $# -eq 0 ]; then
    echo "Usage: $0 <source-profile>"
    echo "Example: $0 housing-production"
    exit 1
fi

SOURCE_PROFILE=$1
TARGET_PROFILE="disaster-recovery"

echo "Migrating environment variables from $SOURCE_PROFILE to $TARGET_PROFILE..."

# Environment variables from serverless.yml
ENV_VARS=(
    "/housing-tl/production/person-api-url"
    "/housing-tl/production/person-api-token"
    "/housing-finance/production/account-api-url"
    "/housing-tl/production/account-api-token"
    "/sqs-queue/production/tenures/arn"
    "/sns-topic/production/person/arn"
    "/sns-topic/production/accounts/arn"
)

for param in "${ENV_VARS[@]}"; do
    echo "Processing parameter: $param"
    
    # Get value from source account
    value=$(aws ssm get-parameter --name "$param" --with-decryption --profile "$SOURCE_PROFILE" --query 'Parameter.Value' --output text 2>/dev/null)
    
    if [ $? -eq 0 ] && [ "$value" != "None" ]; then
        echo "  Retrieved value from source account"
        
        # Put value in target account
        aws ssm put-parameter --name "$param" --value "$value" --type "String" --overwrite --profile "$TARGET_PROFILE"
        
        if [ $? -eq 0 ]; then
            echo "  ✓ Successfully migrated $param"
        else
            echo "  ✗ Failed to migrate $param"
        fi
    else
        echo "  ⚠ Parameter $param not found in source account"
    fi
    echo ""
done

echo "Migration complete!"