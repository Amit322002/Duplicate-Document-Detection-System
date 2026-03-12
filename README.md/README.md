# Duplicate Document Detection System

A full-stack system that detects duplicate or similar documents using a multi-layer detection pipeline combining **hashing, vector embeddings, and computer vision**.

The system supports detecting **exact duplicates and visually similar documents**, even if the file format or compression changes.

---

# 🚀 Features

* Upload documents from a web interface
* Detect **exact duplicates using SHA256 hashing**
* Detect **similar documents using vector embeddings**
* Perform **image similarity validation using ORB (OpenCV)**
* Store embeddings in **Qdrant vector database**
* Convert **PDF and DOCX documents to images**
* Backend built with **ASP.NET Core Web API**
* Frontend built with **React + Vite**

---

# 🧠 Detection Pipeline

The system processes documents using multiple validation layers:

```
Upload File
   ↓
Generate SHA256 Hash
   ↓
Check Exact Duplicate
   ↓
Convert Document → Image
   ↓
Generate Embedding Vector
   ↓
Search Similar Vectors (Qdrant)
   ↓
Validate Using ORB Feature Matching
   ↓
Store Document + Embedding
```

This pipeline allows detection of duplicates even when documents are resized, compressed, or slightly modified.

---

# 🏗 System Architecture

```
Frontend (React / Vite)
        ↓
ASP.NET Core Web API
        ↓
Document Processing Service
        ↓
Embedding Generator
        ↓
Qdrant Vector Database
        ↓
SQL Server Database
```

---

# 🛠 Tech Stack

## Backend

* ASP.NET Core Web API
* Entity Framework Core
* OpenCV (ORB Feature Matching)
* Qdrant Vector Database
* SQL Server

## Frontend

* React
* Vite
* Fetch API

---

# 📂 Project Structure

```
Duplicate-Document-Detection-System
│
├── DuplicateDocsFinder            # Backend (.NET Web API)
│
├── DuplicateDocsFinderfrontend    # Frontend (React + Vite)
│
└── README.md
```

---

# ⚙️ Setup Instructions

## 1️⃣ Clone the Repository

```
git clone https://github.com/YOUR_USERNAME/duplicate-document-detection-system.git
cd duplicate-document-detection-system
```

---

## 2️⃣ Start Qdrant

Run Qdrant locally (Docker example):

```
docker run -p 6333:6333 qdrant/qdrant
```

Create the collection:

```
curl -X PUT http://localhost:6333/collections/files ^
-H "Content-Type: application/json" ^
-d "{\"vectors\":{\"size\":1024,\"distance\":\"Cosine\"}}"
```

---

## 3️⃣ Configure Backend

Update:

```
DuplicateDocsFinder/appsettings.json
```

Example:

```
"ConnectionStrings": {
  "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION"
},
"Qdrant": {
  "Url": "http://localhost:6333",
  "Collection": "files"
},
"DocumentSettings": {
  "StoragePath": "Uploads"
}
```

---

## 4️⃣ Run Backend

```
cd DuplicateDocsFinder
dotnet run
```

Backend will start on:

```
https://localhost:5001
```

---

## 5️⃣ Run Frontend

```
cd DuplicateDocsFinderfrontend
npm install
npm run dev
```

Frontend will run on:

```
http://localhost:5173
```

---

# 📡 API Endpoint

Upload document:

```
POST /api/document/upload_file
```

Form Data:

```
file : document file
userId : user identifier
```

Example response:

```
{
  "success": true,
  "statusCode": 200,
  "message": "Document uploaded successfully"
}
```

---

# 📈 Future Improvements

* Similarity score visualization
* Duplicate document preview
* Cloud storage integration
* Authentication system
* Background processing queue

---

# 👨‍💻 Author

Amit Kumar
.NET Backend Developer

---

⭐ If you found this project useful, consider **starring the repository**.
