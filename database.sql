CREATE DATABASE IF NOT EXISTS catalogo_videojuegos
	CHARACTER SET utf8mb4
	COLLATE utf8mb4_unicode_ci;

USE catalogo_videojuegos;

CREATE TABLE IF NOT EXISTS Usuarios (
	id INT NOT NULL AUTO_INCREMENT,
	nombre_usuario VARCHAR(50) NOT NULL,
	`password` VARCHAR(255) NOT NULL,
	PRIMARY KEY (id),
	UNIQUE KEY uq_usuarios_nombre_usuario (nombre_usuario)
);

INSERT INTO Usuarios (nombre_usuario, `password`)
SELECT 'user1', 'pass1'
WHERE NOT EXISTS (
	SELECT 1 FROM Usuarios WHERE nombre_usuario = 'user1'
);

INSERT INTO Usuarios (nombre_usuario, `password`)
SELECT 'admin', '1234'
WHERE NOT EXISTS (
	SELECT 1 FROM Usuarios WHERE nombre_usuario = 'admin'
);
