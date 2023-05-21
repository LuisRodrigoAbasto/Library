
-- List columns in all tables whose name is like 'TableName'
SELECT 
    [Table_Name] = tbl.table_schema + '.' + tbl.table_name, 
    [Column_Name] = col.column_name, 
    [Column_Type] = col.data_type,
    [Column_Length] = col.CHARACTER_MAXIMUM_LENGTH,
    [Column_Prec] =col.NUMERIC_PRECISION,
    [Column_Scale]=col.NUMERIC_SCALE
    
FROM INFORMATION_SCHEMA.TABLES tbl
INNER JOIN INFORMATION_SCHEMA.COLUMNS col 
    ON col.table_name = tbl.table_name
    AND col.table_schema = tbl.table_schema

WHERE tbl.table_type = 'base table' 
and tbl.table_name='tablename'

