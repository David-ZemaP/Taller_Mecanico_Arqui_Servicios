-- Migration: Agregar Campos de Auditoría a Tabla usuariologin
-- Objetivo: Implementar tracking de usuario y timestamp para CREATE/UPDATE
-- Fecha: 2026-05-06
-- Status: Pendiente Ejecución en BD PostgreSQL 17

BEGIN;

-- Agregar columnas si no existen
ALTER TABLE usuariologin
ADD COLUMN IF NOT EXISTS usuario_creacion VARCHAR(100),
ADD COLUMN IF NOT EXISTS fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
ADD COLUMN IF NOT EXISTS usuario_modificacion VARCHAR(100),
ADD COLUMN IF NOT EXISTS fecha_modificacion TIMESTAMP;

-- Comentarios para documentación
COMMENT ON COLUMN usuariologin.usuario_creacion IS 'Usuario que creó el registro';
COMMENT ON COLUMN usuariologin.fecha_creacion IS 'Timestamp de creación';
COMMENT ON COLUMN usuariologin.usuario_modificacion IS 'Usuario que modificó último';
COMMENT ON COLUMN usuariologin.fecha_modificacion IS 'Timestamp de última modificación';

-- Poblar campos faltantes en registros existentes (si aplica)
UPDATE usuariologin 
SET usuario_creacion = 'SYSTEM', 
    fecha_creacion = NOW()
WHERE usuario_creacion IS NULL;

COMMIT;

-- Verificación post-migración:
-- SELECT usuario_id, usuario_creacion, fecha_creacion, usuario_modificacion, fecha_modificacion 
-- FROM usuariologin LIMIT 5;
