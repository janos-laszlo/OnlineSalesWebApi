To use this project:
-Add the following to the configuration:
    -'ConnectionStrings:MariaDB': a connection string to MariaDB
    -'Jwt:EncryptionKey': the key used to encrypt the JWT
-call AddUserIdentity on the service collection

TODO:
-remove users who haven't confirmed their email after a certain time