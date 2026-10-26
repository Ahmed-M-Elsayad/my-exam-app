# Security Policy

## Supported versions

Only the latest code on the main branch gets fixes.

## Reporting a vulnerability

Please do not open a public issue for security problems. Email **a.elsuad08@gmail.com** instead and include:

- what you found
- how to reproduce it
- what an attacker could do with it
- a fix, if you have one

I maintain this project alone, so replies are best effort. I aim to acknowledge a report within 72 hours; how long a fix takes depends on the severity. If you want credit in the release notes, say so.

## What the app does

**Answers stay on the server until you submit.** During an exam the API sends `ExamQuestionDto`, which has no correct answers. `ReviewQuestionDto`, which does, is only returned from the evaluate endpoint.

**Grading is server-side.** The score is computed in `ExamService.SubmitAsync`. The client only sends which option it picked (by original index) or the free text.

**Rate limiting** uses ASP.NET Core's built-in rate limiter, with a fixed one-minute window per client IP. Going over the limit returns `429 Too Many Requests`.

- `GET /api/questions` and `GET /api/questions/categories` share one limit of 30 requests per minute
- `POST /api/exams/evaluate`: 5 requests per minute

**Input validation.** `ExamSubmissionDto` and `ExamAnswerDto` use DataAnnotations: between 1 and 100 answers per submission, free text up to 5000 characters, question id of at least 1, option index between 0 and 100. Invalid requests are rejected with `400` before they reach the service. Inside `SubmitAsync`, an option index outside the question's options counts as not answered, unknown question ids are logged and skipped, and a question that appears twice in one submission is only counted once.

**Security headers** are added by middleware: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` and `Permissions-Policy` on every response, plus `Content-Security-Policy`. API responses get `default-src 'none'`. The page gets a policy that only allows scripts, styles, images and connections from the same origin. The `/swagger` path is excluded because Swagger UI needs inline scripts.

**Database access** goes through Entity Framework Core, so queries are parameterized. There is no hand-built SQL.

**Frontend.** All question and feedback text is passed through `escapeHtml()` before it is inserted as HTML, and the CSP only allows scripts and styles from the same origin.

**HTTPS.** In production the app sends an HSTS header with a one-year max-age, and HTTP requests are redirected to HTTPS.

**Stored data.** Each submission is saved as an exam attempt: the score, and for every MCQ or true/false question the option that was picked and whether it was right. Essay and report text is not stored. Nothing ties an attempt to a user.

**Database file.** The SQLite file (`examapp.db`) is created next to the app, outside `wwwroot`, so it is not served as a static file, and it is in `.gitignore`. It contains the correct answers, so keep it that way.

## Known limitations

- There are no user accounts. Anyone can submit an exam and see the correct answers afterwards. The protection is that answers cannot be read from the API before submitting, which stops casual DevTools peeking, not a determined person.
- The seed file `Data/questions.json` is in the repository, so the answers are public to anyone who reads the source.
- `GET /api/questions` returns every matching question (without answers). Picking the 15 questions for an exam happens in the browser, so the server does not enforce exam size or composition.
- Attempts are never cleaned up, so the database grows with use.
- `AllowedHosts` is `*` in `appsettings.json`. Set it to your real domain when deploying.
- Rate limiting is the only defense against scripted use of the API. It is keyed on the IP address the server sees, so behind a reverse proxy the forwarded-headers middleware has to be configured or every visitor shares one limit.
