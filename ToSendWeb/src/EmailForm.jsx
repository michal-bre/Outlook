import React, { useState } from "react";
import "./EmailForm.css";

const API_BASE = "https://localhost:7232";

function EmailForm() {
  const [to, setTo] = useState("");
  const [subject, setSubject] = useState("");
  const [body, setBody] = useState("");
  const [cv, setCv] = useState(null);
  const [status, setStatus] = useState("");
  const [token, setToken] = useState("");
  const [errors, setErrors] = useState({});

  function validateForm() {
    const newErrors = {};

    if (!to.trim()) {
      newErrors.to = "שדה נמען חובה";
    } else {
      const emails = to
        .split(",")
        .map((e) => e.trim())
        .filter((e) => e.length > 0);

      const invalidEmails = emails.filter(
        (email) => !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
      );

      if (invalidEmails.length > 0) {
        newErrors.to = `כתובות דוא״ל לא תקינות: ${invalidEmails.join(", ")}`;
      }
    }

    if (!subject.trim()) {
      newErrors.subject = "שדה נושא חובה";
    }

    if (!body.trim()) {
      newErrors.body = "שדה גוף ההודעה חובה";
    }

    if (!cv) {
      newErrors.cv = "חובה לבחור קובץ קורות חיים";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setStatus("");
    setToken("");

    if (!validateForm()) return;

    const formData = new FormData();
    formData.append("To", to);
    formData.append("Subject", subject);
    formData.append("Body", body);
    formData.append("Cv", cv);

    try {
      setStatus("שולח לשרת...");

      const res = await fetch(`${API_BASE}/api/EmailDraft`, {
        method: "POST",
        body: formData,
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "שגיאה בשליחה לשרת");
      }

      const data = await res.json();
      setToken(data.token);
      setStatus("הטיוטות נוצרו ונשמרו, אפשר לעבור ל-Outlook.");

      // הפעלת ה-Helper המקומי
      try {
        window.location.href = `jobmail://create?token=${data.token}`;
      } catch (e) {
        console.error("בעיה בפתיחת ה-Helper:", e);
      }

      setTo("");
      setSubject("");
      setBody("");
      setCv(null);
      setErrors({});
      setToken("");

    } catch (err) {
      console.error(err);
      setStatus("אירעה שגיאה: " + err.message);
    }
  }

  function handleFileChange(e) {
    const file = e.target.files?.[0];
    setCv(file || null);

    setErrors((prev) => ({
      ...prev,
      cv: undefined,
    }));
  }

  return (
    <div className="email-page">
      <div className="email-card">
        <div className="email-header">
          <div className="email-pill">מחולל טיוטות ל-Outlook</div>
          <h2>שליחת קורות חיים במייל</h2>
          <p>ממלאים פעם אחת — מקבלים טיוטות מייל מוכנות, אחת לכל נמען.</p>
        </div>

        <form className="email-form" onSubmit={handleSubmit}>
          {/* נמען */}
          <div className="form-group">
            <label>
              נמען/ים
              <span className="required">*</span>
            </label>
            <input
              type="text"
              value={to}
              onChange={(e) => {
                setTo(e.target.value);
                if (errors.to) {
                  setErrors((prev) => ({ ...prev, to: undefined }));
                }
              }}
              className={errors.to ? "input error" : "input"}
              placeholder="לדוגמה: user1@mail.com, user2@mail.com"
            />
            <div className="field-hint">
              אפשר להזין כמה כתובות, מופרדות בפסיק.
            </div>
            {errors.to && <div className="error-text">{errors.to}</div>}
          </div>

          {/* נושא */}
          <div className="form-group">
            <label>
              נושא
              <span className="required">*</span>
            </label>
            <input
              type="text"
              value={subject}
              onChange={(e) => {
                setSubject(e.target.value);
                if (errors.subject) {
                  setErrors((prev) => ({ ...prev, subject: undefined }));
                }
              }}
              className={errors.subject ? "input error" : "input"}
              placeholder="לדוגמה: הגשת מועמדות למשרת ... "
            />
            {errors.subject && (
              <div className="error-text">{errors.subject}</div>
            )}
          </div>

          {/* גוף הודעה */}
          <div className="form-group">
            <label>
              גוף ההודעה
              <span className="required">*</span>
            </label>
            <textarea
              value={body}
              onChange={(e) => {
                setBody(e.target.value);
                if (errors.body) {
                  setErrors((prev) => ({ ...prev, body: undefined }));
                }
              }}
              className={errors.body ? "textarea error" : "textarea"}
              placeholder="שלום, מצורפים קורות החיים שלי עבור משרת..."
            />
            {errors.body && <div className="error-text">{errors.body}</div>}
          </div>

          {/* קובץ קורות חיים */}
          <div className="form-group">
            <label>
              קורות חיים (PDF / DOC / DOCX)
              <span className="required">*</span>
            </label>
            <div className="file-wrapper">
              <input
                type="file"
                id="cv-input"
                onChange={handleFileChange}
                accept=".pdf,.doc,.docx"
              />
              <label htmlFor="cv-input" className="file-button">
                בחרי קובץ
              </label>
              <span className="file-name">
                {cv ? cv.name : "לא נבחר קובץ"}
              </span>
            </div>
            {errors.cv && <div className="error-text">{errors.cv}</div>}
          </div>

          {/* כפתור */}
          <button type="submit" className="submit-button">
            שמור טיוטות ב-Outlook
          </button>

          {/* סטטוס + טוקן */}
          {status && (
            <div
              className={
                status.includes("שגיאה") ? "status status-error" : "status status-ok"
              }
            >
              {status}
            </div>
          )}

          {token && (
            <div className="status status-token">
              <span>Token שנוצר:</span> <code>{token}</code>
            </div>
          )}
        </form>
      </div>
    </div>
  );
}

export default EmailForm;
