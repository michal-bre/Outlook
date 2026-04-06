Mail Drafts for Outlook (מחולל טיוטות מייל)

פרויקט קטן שמאפשר למלא טופס (Frontend) עם פרטי מייל וקובץ קורות חיים, לשמור "טיוטות" בצד השרת ולפתוח אותן ב-Outlook כטיוטות מוכנות — אחת לכל נמען.

הרכיבים בפרויקט
- **MailDraftApi**: Web API ב-.NET (net8) שמקבל טופס עם שדה קובץ (Cv), שומר את הקובץ בתיקיית `Uploads/Cv` ומחזיר `token` לשיחזור הטיוטה. משמש גם כשרת סטטי לשרת את הקובץ תחת `/uploads/Cv/{file}`.
- **ToSendWeb**: אפליקציית React + Vite שמציגה טופס למשתמש ושולחת את הנתונים ל-API. לאחר יצירת הטיוטות, מנסה לפתוח כתובת פרוטוקול `jobmail://create?token=...` כדי להפעיל את ה-Helper המקומי.
- **MailDraftHelper**: אפליקציית קונסול .NET שמשתמשת ב-API כדי לקבל את הטיוטה (באמצעות `token`), מורידה את קובץ ה-CV ושומרת/פותחת טיוטות ב-Outlook (מייצרת דראפט נפרד לכל נמען).

תנאים מוקדמים
- .NET 8 SDK מותקן (לבניית ה-API וה-Helper).
- Node.js (ל-Frontend) — מומלץ Node 18+.
- Outlook מותקן על המחשב שבו רוצים ש-`MailDraftHelper` יפתח טיוטות.

הרצה מקומית (פירוט)

1) להריץ את ה-API

- עבור אל התיקייה: `MailDraftApi/MailDraftApi`
- התקן תלותיות (אופציונלי):

```powershell
dotnet restore
```

- להפעיל בפיתוח (יפעיל Swagger ו-HTTPS לפי הפרופילים):

```powershell
dotnet run
```

- ברירת המחדל מה-`launchSettings.json` מגדירה `https://localhost:7232` ו-`http://localhost:5102`. ה-API מציע Swagger ב-`/swagger` בפיתוח.

ה-API מקבל POST ל-`/api/EmailDraft` (multipart/form-data) עם שדות: `To`, `Subject`, `Body`, `Cv` (קובץ). התגובה מכילה `{ token }` שמייצג את הטיוטה שנשמרה בזיכרון השרת.

2) להפעיל את ה-Frontend

- עבור אל התיקייה: `ToSendWeb`
- התקן חבילות ו-הפעל את dev server של Vite:

```bash
npm install
npm run dev
```


- לפני הרצה: אפשר להגדיר את הכתובת של ה-API דרך משתנה סביבה ל-Vite (`VITE_API_BASE`). העתק את `ToSendWeb/.env.example` ל-`ToSendWeb/.env` ושנה לפי הצורך. לדוגמה:

```powershell
cd ToSendWeb
copy .env.example .env
# ואז ערוך את .env אם רוצים כתובת שונה
```

- ממשק ה-UI רץ ב-`http://localhost:5173` (ברירת מחדל של Vite). ה-Frontend יקרא את `VITE_API_BASE` (אם קיים) אחרת יחזור ל-`https://localhost:7232`. אם תשנה פורט/כתובת ה-API — עדכן גם את הגדרות CORS ב-API.

3) להריץ את ה-Helper (ליצירת הטיוטות ב-Outlook)

- ה-Helper הוא אפליקציית קונסול שנכתבה ב-.NET ומניחה שיש Outlook מותקן.
- אפשר להריץ אותו ידנית (לא דרך פרוטוקול) על-ידי מתן ה-URL המותאם כפרמטר. הדוגמה משתמשת ב-URL מסוג `jobmail://create?token=...` — בתוך ה-Helper זה מוחלף ל-`https://...` ואז נקרא ה-API.

דוגמה להרצה ידנית (מפתח/פיתוח):

```powershell
cd MailDraftHelper/MailDraftHelper
dotnet run -- "jobmail://create?token=THE_TOKEN_HERE"
```

- או לפרסם ולרוץ את ה-.exe ולקרוא לו עם אותו הפרמטר:

```powershell
dotnet publish -c Release -r win-x64 -o publish
"publish\MailDraftHelper.exe" "jobmail://create?token=THE_TOKEN_HERE"
```

רישום פרוטוקול מותאם (`jobmail://`) ב-Windows (אופציונלי)
- אם רוצים שה-Frontend יקרא ישירות את ה-Helper דרך `jobmail://...`, ניתן לרשום פרוטוקול במערכת. זה יגרום ל-Windows לקרוא את ה-Helper כאשר דפדפן מנסה לפתוח `jobmail://...`.
- דוגמה לרישום פרוטוקול ברמת המשתמש (החלף את הנתיב ל-`MailDraftHelper.exe` הפרסום שלך):

```powershell
REG ADD HKCU\Software\Classes\jobmail /ve /d "URL:jobmail Protocol"
REG ADD HKCU\Software\Classes\jobmail /v "URL Protocol" /d ""
REG ADD HKCU\Software\Classes\jobmail\shell\open\command /ve /d "\"C:\\path\\to\\MailDraftHelper.exe\" \"%1\""
```

- לאחר רישום, לחיצה על קישור `jobmail://create?token=...` תפעיל את ה-Helper עם הפרמטר.

תהליך עבודה לדוגמה
1. הפעל את ה-API (`dotnet run`) וה-Frontend (`npm run dev`).
2. פתח את ה-UI ב-`http://localhost:5173`, מלא `To`, `Subject`, `Body` ובחר קובץ CV.
3. שלח — ה-Frontend יבצע POST ל-API، ה-API ישמור את הקובץ ב-`Uploads/Cv` ויחזיר `token`.
4. ה-Frontend ינסה לפתוח `jobmail://create?token=...` — אם הפרוטוקול רשום, ה-Helper יקבל את ה-URL, יחלץ את הטוקן, יקרא את ה-API לקבלת תוכן הטיוטה, יוריד את ה-CV ויפתח טיוטות ב-Outlook (אחת לכל נמען).

הערות ופתרון תקלות
- ה-API שומר את הטיוטות בזיכרון (`EmailDraftStore`), כלומר אחרי הפעלה מחדש של השרת הטיוטות יאבדו.
- ודאו שה-API רץ ב-HTTPS כפי שמצופה בקוד של ה-Helper (`https://localhost:7232`). אם תשנו פורט/פרוטוקול — עדכנו גם את `ToSendWeb/src/EmailForm.jsx` (המשתנה `API_BASE`) ואת ה-Helper במידת הצורך.
- אם ה-Helper לא מצליח לפתוח Outlook — ודאו ש-Outlook מותקן ושהיישום מופעל בהרשאות המתאימות.
- הקבצים הנשמרים נמצאים בתיקייה `MailDraftApi/MailDraftApi/Uploads/Cv` בזמן הריצה.
