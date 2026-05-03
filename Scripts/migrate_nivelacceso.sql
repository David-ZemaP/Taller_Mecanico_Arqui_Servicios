-- Migration script to update NivelAcceso values from old format to new enum values
-- Run this script to migrate existing data to the new NivelAcceso enum system

-- Update 'Total' to 'Completo' for all administrators
UPDATE empleado 
SET nivelacceso = 'Completo' 
WHERE nivelacceso = 'Total' 
  AND tipoempleado = 'Administrador';

-- Set the default administrator (CI: 100000) to 'Gerente' level
UPDATE empleado 
SET nivelacceso = 'Gerente' 
WHERE ci = 100000 
  AND tipoempleado = 'Administrador';

-- Verify the changes
SELECT ci, nombre, apellido, nivelacceso, tipoempleado 
FROM empleado 
WHERE tipoempleado = 'Administrador'
ORDER BY nivelacceso, ci;
