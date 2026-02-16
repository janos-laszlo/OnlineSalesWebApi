To use this project:
-Add the following to the configuration:
    -'ConnectionStrings:MariaDB': a connection string to MariaDB
    -'Jwt:EncryptionKey': the key used to encrypt the JWT
    -'BaseUrl': the web API base URL
    -'DataProtection:EmailConfirmationTokenPurpose' - key to encrypt/decrypt email confirmation token
-call AddUserIdentity on the service collection
-call UseUserIdentity on the IApplicationBuilder
