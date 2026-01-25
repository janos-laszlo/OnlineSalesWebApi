To use this project:
-Add the following to the configuration:
    -'ConnectionStrings:MariaDB': a connection string to MariaDB
    -'Jwt:EncryptionKey': the key used to encrypt the JWT
-call AddUserIdentity on the service collection
-call UseUserIdentity on the IApplicationBuilder

TODO:
-refresh tokens:
    -the refresh token will have information about the user ID and 
    expiry timestamp. It will be encrypted using a custom key + user ID
    -inspire from the following to encrypt the refresh token contenxt
        https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/using-data-protection?view=aspnetcore-10.0
        https://medium.com/@muhebollah.diu/securing-query-parameters-in-asp-net-core-encrypting-guids-for-better-security-fdfd921033ae
        https://www.c-sharpcorner.com/blogs/securing-the-url-parameterother-sensitive-data-using-net-core-dataprotectortokenprovider
        https://github.com/ullmark/hashids.net
        https://stackoverflow.com/questions/75516793/how-can-i-use-dataprotectionprovider-to-encrypt-and-validate-user-details
-add global error handler
-return encrypted user IDs: https://www.youtube.com/watch?v=tSuwe7FowzE