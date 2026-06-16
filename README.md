# Engineering Interactive Exam
An interactive engineering exam platform combining MCQ, True/False, short essay, and technical report questions. It's designed to help engineering students review their technical report writing and engineering English curriculum in a random and interactive way.
## ✨ Key Features
- 15 randomly selected questions from a pool of 219 real questions (no duplication in any single exam).
- Filter by topic (Grammar, CV, Motivation, Recommendation, Intro, Structure, Style, Mistakes, Exam, Clauses, Comprehension, Writing, Technical, General, IEEE, Methodology, Implementation, Discussion, Visualization, IoT, Renewable, Robotics, Control, Literature).
- Filter by question type (MCQ – True/False – Essay – Report).
- Instant MCQ/True-False evaluation with correct and incorrect answers highlighted (green/red) in day and night mode.
- A writing area for essay and report answers with the option to view a sample answer and self-assess using keyword matching.
- Day/Night mode (Dark/Light mode).
- A results screen displaying the score percentage, mistakes, and their explanations.
- Frontend input validation and a rate limiter on exam submission.
- Full keyboard support (Tab/Shift+Tab/Enter/Space).
## 🛠️ Technologies Used
- HTML5 + CSS3 (Flex/Grid responsive design).
- JavaScript (ES6).
- JSON (question database).
- GitHub Pages for publishing.
## 📂 Project Structure
my-exam-app/
├── index.html # Main interface and application logic
├── questions.json # Question database
├── LICENSE # License terms and copyright information
└── README.md # This file
## 🚀 How to Run
### Locally (Offline)
1. Download the `index.html` and `questions.json` files (and optionally `LICENSE` for legal reference) into the same folder.
2. Open `index.html` using any modern browser (using a live server is recommended, since the question bank is loaded via `fetch()` and most browsers block this for files opened directly via `file://`).
### Via GitHub Pages
1. Upload the files to a public repository on GitHub.
2. Enable GitHub Pages from Settings (Settings → Pages → Branch: main).
3. Open the link below.
## 🧪 Test Link
https://ahmed-m-elsayad.github.io/my-exam-app/
## 👨‍💻 Developer
**Ahmed Al-Sayad**
Engineering Student – Higher Institute of Engineering and Technology, Kafr El-Sheikh
[LinkedIn Profile](https://www.linkedin.com/in/ahmed-mohammed-ahmed-saber-abdelhamid-b46249324)
## 📄 License
This project is released under a **custom educational license**.  
All rights are reserved by the author.  
You are permitted to use, modify, and distribute the code **for educational and non-commercial purposes only**, provided that proper attribution is given to the original author.
📜 For full terms, see the [LICENSE](./LICENSE) file.
