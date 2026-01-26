-- User tablosuna DoctorId kolonunu ekle
ALTER TABLE Users
ADD DoctorId INT NULL;

-- Foreign Key ekle
ALTER TABLE Users
ADD CONSTRAINT FK_Users_Doctors
FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId);

-- Kontrol
SELECT * FROM Users;
