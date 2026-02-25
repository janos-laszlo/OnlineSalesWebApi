SELECT FULL_COLLATION_NAME AS `Collation`, CHARACTER_SET_NAME AS `Charset`, ID AS `Id`, IS_DEFAULT AS `Default`, 0 AS `Sortlen` FROM `information_schema`.COLLATION_CHARACTER_SET_APPLICABILITY ORDER BY `Collation`;
CREATE DATABASE `online_sales` /*!40100 COLLATE 'utf8mb4_uca1400_ai_ci' */;

CREATE USER online_sales_user IDENTIFIED BY 'SalesAre c00l';
GRANT USAGE ON online_sales.* TO online_sales_user IDENTIFIED BY 'SalesAre c00l';
GRANT ALL PRIVILEGES ON online_sales.* TO 'online_sales_user'@'%';

SELECT FULL_COLLATION_NAME AS `Collation`, CHARACTER_SET_NAME AS `Charset`, ID AS `Id`, IS_DEFAULT AS `Default`, 0 AS `Sortlen` FROM `information_schema`.COLLATION_CHARACTER_SET_APPLICABILITY ORDER BY `Collation`;
CREATE DATABASE `online_sales_integration_tests` /*!40100 COLLATE 'utf8mb4_uca1400_ai_ci' */;

CREATE USER online_sales_user_integration_tests IDENTIFIED BY 'SalesAre c00o0o0l';
GRANT USAGE ON online_sales_integration_tests.* TO online_sales_user_integration_tests IDENTIFIED BY 'SalesAre c00o0o0l';
GRANT ALL PRIVILEGES ON online_sales_integration_tests.* TO 'online_sales_user_integration_tests'@'%';
