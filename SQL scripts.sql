CREATE USER online_sales_user IDENTIFIED BY 'SalesAre c00l';
GRANT USAGE ON online_sales.* TO online_sales_user IDENTIFIED BY 'SalesAre c00l';
GRANT ALL PRIVILEGES ON online_sales.* TO 'online_sales_user'@'%';

CREATE USER online_sales_user_integration_tests IDENTIFIED BY 'SalesAre c00o0o0l';
GRANT USAGE ON online_sales_integration_tests.* TO online_sales_user_integration_tests IDENTIFIED BY 'SalesAre c00o0o0l';
GRANT ALL PRIVILEGES ON online_sales_integration_tests.* TO 'online_sales_user_integration_tests'@'%';
