CREATE DATABASE IF NOT EXISTS library_db;
USE library_db;

CREATE TABLE IF NOT EXISTS books (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    author VARCHAR(255) NOT NULL,
    isbn VARCHAR(30) NOT NULL UNIQUE,
    publication_year INT NOT NULL,
    genre VARCHAR(120) NOT NULL,
    shelf_section VARCHAR(80) NOT NULL,
    shelf_number VARCHAR(80) NOT NULL,
    is_available BOOLEAN NOT NULL DEFAULT TRUE
);

INSERT INTO books (title, author, isbn, publication_year, genre, shelf_section, shelf_number, is_available)
VALUES
('Le Petit Prince', 'Antoine de Saint-Exupery', '9780156013987', 1943, 'Roman', 'A', '1', TRUE),
('1984', 'George Orwell', '9780451524935', 1949, 'Dystopie', 'B', '3', TRUE),
('Clean Code', 'Robert C. Martin', '9780132350884', 2008, 'Informatique', 'C', '7', FALSE);
