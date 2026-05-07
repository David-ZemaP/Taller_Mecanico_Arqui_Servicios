-- Migración: agregar campo username y columnas de auditoría a usuariologin
-- Ejecutar UNA SOLA VEZ contra la base de datos TallerMecanico

-- 1. Columna username (generado automáticamente por UsersService)
ALTER TABLE usuariologin ADD COLUMN IF NOT EXISTS username VARCHAR(50);

-- 2. Columnas de auditoría
ALTER TABLE usuariologin ADD COLUMN IF NOT EXISTS usuario_creacion VARCHAR(100);
ALTER TABLE usuariologin ADD COLUMN IF NOT EXISTS fecha_creacion TIMESTAMP DEFAULT NOW();
ALTER TABLE usuariologin ADD COLUMN IF NOT EXISTS usuario_modificacion VARCHAR(100);
ALTER TABLE usuariologin ADD COLUMN IF NOT EXISTS fecha_modificacion TIMESTAMP;

-- 3. Índice único en username (para búsquedas rápidas y evitar duplicados)
CREATE UNIQUE INDEX IF NOT EXISTS uq_usuariologin_username ON usuariologin(username)
    WHERE username IS NOT NULL;

-- 4. Poblar username para registros existentes (inicial + primer apellido)
-- Requiere ajuste manual si no existe tabla empleado con estos campos

-- 5. Columnas de auditoría en ordentrabajo (si no existen)
ALTER TABLE ordentrabajo ADD COLUMN IF NOT EXISTS creadopor VARCHAR(100);
ALTER TABLE ordentrabajo ADD COLUMN IF NOT EXISTS actualizadopor VARCHAR(100);
ALTER TABLE ordentrabajo ADD COLUMN IF NOT EXISTS eliminadopor VARCHAR(100);

-- 6. Columnas de auditoría en ordentrabajoproducto y ordentrabajoservicio
ALTER TABLE ordentrabajoproducto ADD COLUMN IF NOT EXISTS creadopor VARCHAR(100);
ALTER TABLE ordentrabajoservicio ADD COLUMN IF NOT EXISTS creadopor VARCHAR(100);
ALTER TABLE ordentrabajocatalogo ADD COLUMN IF NOT EXISTS creadopor VARCHAR(100);
