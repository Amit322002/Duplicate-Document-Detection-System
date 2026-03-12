import React, { useState, useRef } from 'react';
import { UploadCloud, CheckCircle, AlertCircle, File, X, Loader2 } from 'lucide-react';
import './index.css';

function App() {
  const [file, setFile] = useState(null);
  const [isDragging, setIsDragging] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [status, setStatus] = useState(null); // 'success' | 'error' | null
  const [message, setMessage] = useState('');
  const [userId, setUserId] = useState('2'); // Add userId state
  
  const fileInputRef = useRef(null);

  const handleDragOver = (e) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setIsDragging(false);
    
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      setFile(e.dataTransfer.files[0]);
      setStatus(null);
      setMessage('');
    }
  };

  const handleFileChange = (e) => {
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
      setStatus(null);
      setMessage('');
    }
  };

  const handleRemoveFile = () => {
    setFile(null);
    setStatus(null);
    setMessage('');
  };

  const handleUpload = async () => {
    if (!file) return;

    setIsUploading(true);
    setStatus(null);
    setMessage('');

    const formData = new FormData();
    formData.append('UserId', userId);
    formData.append('File', file);

    try {
      // Strictly read from the environment variable 
      const baseUrl = import.meta.env.VITE_API_BASE_URL;
      const response = await fetch(`${baseUrl}/api/documents/upload_file`, {
        method: 'POST',
        headers: {
          'accept': '*/*'
        },
        body: formData,
      });

      let data = {};
      try {
        // Try parsing JSON, but don't fail if the response is empty (like a 404)
        data = await response.json();
      } catch (parseError) {
        console.warn('Could not parse response as JSON');
      }
      
      if (response.ok && data.success !== false) {
        setStatus('success');
        setMessage(data.message || 'File uploaded successfully!');
      } else {
        // Here we handle HTTP status like 409 or any JSON response errors
        setStatus('error');
        setMessage(data.message || `Upload failed with status: ${response.status}`);
      }
    } catch (error) {
      console.error('Upload Error:', error);
      setStatus('error');
      setMessage('Network error. Make sure the backend server and CORS are correctly configured.');
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="app-container">
      <div className="glass-card">
        <div className="header">
          <h1>Document Uploader</h1>
          <p>Upload a file securely to check for duplicates</p>
        </div>

        <div className="user-input-group">
          <label htmlFor="userId">User ID</label>
          <input 
            type="text" 
            id="userId" 
            value={userId} 
            onChange={(e) => setUserId(e.target.value)}
            className="user-id-input"
            placeholder="Enter User ID"
          />
        </div>

        <div className="upload-section">
          {!file ? (
            <div 
              className={`drop-zone ${isDragging ? 'dragging' : ''}`}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
              onClick={() => fileInputRef.current.click()}
            >
              <UploadCloud className="upload-icon" size={56} />
              <h3>Drag & Drop your file here</h3>
              <p>or click to browse from your computer</p>
              <input 
                type="file" 
                ref={fileInputRef} 
                onChange={handleFileChange} 
                className="hidden-input"
              />
            </div>
          ) : (
            <div className="file-preview">
              <div className="file-info">
                <File className="file-icon" size={36} />
                <div className="file-details">
                  <span className="file-name" title={file.name}>{file.name}</span>
                  <span className="file-size">{(file.size / 1024 / 1024).toFixed(2)} MB</span>
                </div>
                {!isUploading && (
                  <button className="remove-btn" onClick={handleRemoveFile} title="Remove file">
                    <X size={20} />
                  </button>
                )}
              </div>

              {status && (
                <div className={`status-alert ${status}`}>
                  {status === 'success' ? (
                    <CheckCircle size={22} style={{ flexShrink: 0 }} />
                  ) : (
                    <AlertCircle size={22} style={{ flexShrink: 0 }} />
                  )}
                  <span>{message}</span>
                </div>
              )}

              <button 
                className="upload-btn" 
                onClick={handleUpload} 
                disabled={isUploading || status === 'success'}
              >
                {isUploading ? (
                  <>
                    <Loader2 className="spinner" size={20} />
                    Processing Upload...
                  </>
                ) : status === 'success' ? (
                  'Uploaded Succesfully'
                ) : (
                  'Upload Document'
                )}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
