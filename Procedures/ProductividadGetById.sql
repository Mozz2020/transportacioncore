IF EXISTS (
	SELECT *
	FROM INFORMATION_SCHEMA.ROUTINES
	WHERE ROUTINE_TYPE = 'PROCEDURE'
	AND ROUTINE_SCHEMA = 'dbo'
	AND ROUTINE_NAME = 'ProductividadGetById'
)
BEGIN
	DROP PROCEDURE dbo.ProductividadGetById;
END;

Go 
CREATE Procedure [dbo].[ProductividadGetById]
	@IdPlanificacion AS decimal(18,0)
As

DECLARE @cols  AS NVARCHAR(MAX);
DECLARE @query AS NVARCHAR(MAX);

SET @cols = STUFF((SELECT ',' + QUOTENAME(Dia) 
            FROM (
                SELECT DISTINCT Dia
                FROM vw_Productividad
                WHERE IdPlanificacion = @IdPlanificacion
            ) t
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'),1,1,'')

SET @query = 'SELECT IdPlanificacion, IdOperador, IdCliente, IdTienda, NombreTienda, NombreCliente, NombreEstado, NombreOperador, Spot,TipoRegistro, ' + @cols + ' 
            FROM 
            (
                SELECT 
                    IdPlanificacion
                  , IdOperador
                  , IdCliente
                  , IdTienda
                  , NombreTienda
                  , NombreCliente
                  , NombreEstado
                  , NombreOperador    
                  , Spot
                  , Dia
				  , TipoRegistro
                FROM vw_Productividad 
                WHERE IdPlanificacion = ' + CAST(@idPlanificacion AS NVARCHAR(10)) + '
           ) x
            PIVOT 
            (
                MIN(Dia)
                FOR Dia IN (' + @cols + ')
            ) p '

EXECUTE(@query);
