To use this project:
-Add the following to the configuration:
    -'ConnectionStrings:MariaDB': a connection string to MariaDB
    -"R2": {
        "AccountId": "",
        "AccessKeyId": "",
        "SecretAccessKey": ""
      }
-call AddVehicleSales on the service collection
-it depends on the UserIdentity.User entity/table existing.

-create vehicle sale
    -POST /vehicle-sales -> response: { vehicleSaleId, directoryId, objectUploads: [{objectKey1, presignedUploadUrl},...] }
    -POST /confirm-file-upload -> request: { directoryId, objectKeys: [ objectKey1, ... ], behavior: 'append|replace'}
