-- Migración para agregar soporte de usuarios de cliente
-- Ejecutar en PostgreSQL (puerto 5433)

-- 1. Agregar columna Email a Cliente si no existe o es nullable
ALTER TABLE cliente ALTER COLUMN email SET NOT NULL;

-- 2. Agregar UsuarioLoginId a Cliente
ALTER TABLE cliente ADD COLUMN IF NOT EXISTS usuariologinid INT NULL;
ALTER TABLE cliente ADD CONSTRAINT FK_Cliente_UsuarioLogin FOREIGN KEY (usuariologinid) REFERENCES usuariologin(usuariologinid);

-- 3. Modificar UsuarioLogin para soportar clientes (empleadoid nullable)
ALTER TABLE usuariologin ALTER COLUMN empleadoid DROP NOT NULL;

-- 4. Agregar ClienteId y EsCliente a UsuarioLogin
ALTER TABLE usuariologin ADD COLUMN IF NOT EXISTS clienteid INT NULL UNIQUE;
ALTER TABLE usuariologin ADD COLUMN IF NOT EXISTS escliente BOOLEAN NOT NULL DEFAULT FALSE;

-- 5. Agregar constraints
ALTER TABLE usuariologin ADD CONSTRAINT FK_UsuarioLogin_Cliente FOREIGN KEY (clienteid) REFERENCES cliente(clienteid) ON DELETE CASCADE;

-- Note: El check constraint ya existente puede necesitar ajuste
-- Si hay error, ejecutar:
-- ALTER TABLE usuariologin DROP CONSTRAINT IF EXISTS ck_usuariologin_empleadoorcliente;
-- ALTER TABLE usuariologin ADD CONSTRAINT ck_usuariologin_empleadoorcliente CHECK (
--     (empleadoid IS NOT NULL AND clienteid IS NULL) OR 
--     (empleadoid IS NULL AND clienteid IS NOT NULL)
-- );