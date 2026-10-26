import { fetchQuestions, fetchCategories, submitExamToServer } from './api.js';

const MAX_QUESTIONS = 15;
const THEME_KEY = 'exam-theme';

const TYPE_LABELS = { mcq: 'MCQ', tf: 'True/False', essay: 'Essay', report: 'Report' };

const state = {
    exam: [],
    answers: {},
    review: {},
    order: {},
    submitted: false,
    filters: { category: 'all', type: 'all' }
};

// Helpers
const $ = id => document.getElementById(id);

function escapeHtml(str) {
    if (!str) return '';
    return String(str).replace(/[&<>"']/g, match => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;'
    }[match]));
}

function shuffleArray(arr) {
    const copy = [...arr];
    for (let i = copy.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [copy[i], copy[j]] = [copy[j], copy[i]];
    }
    return copy;
}

// Load Data
let loadId = 0;
let categoriesCache = null; // التصنيفات مبتتغيرش، مفيش داعي نطلبها مع كل فلتر

async function initExam() {
    const myId = ++loadId; // لو المستخدم داس فلتر تاني قبل ما الأول يخلص، نتجاهل القديم
    $('loadStatus').textContent = 'Loading questions...';
    $('submitExamBtn').disabled = false;

    try {
        const [questions, categories] = await Promise.all([
            fetchQuestions(state.filters.category, state.filters.type),
            categoriesCache ?? fetchCategories()
        ]);
        if (myId !== loadId) return;
        categoriesCache = categories;

        // خلط الأسئلة واختيار 15
        state.exam = shuffleArray(questions).slice(0, MAX_QUESTIONS);
        state.answers = {};
        state.review = {};
        state.order = {};
        state.submitted = false;

        renderFilters(categories);
        renderExam();
        updateProgress();

        $('loadStatus').textContent = '';
        $('resultModal').style.display = 'none';
    } catch (err) {
        if (myId !== loadId) return;
        console.error(err);
        $('loadStatus').textContent = err.status === 429
            ? 'Too many requests. Wait a minute and try again.'
            : 'Error loading questions. Please refresh.';
    }
}

// Render
function renderExam() {
    const area = $('questionsArea');
    if (!state.exam.length) {
        area.innerHTML = '<p class="empty-state">No questions match your filters.</p>';
        return;
    }

    area.innerHTML = state.exam.map((q, i) => buildCard(q, i)).join('');
    attachEvents();
}

function buildCard(q, index) {
    const saved = state.answers[q.id];
    const rev = state.review[q.id];
    const isChoice = q.type === 'mcq' || q.type === 'tf';

    const body = isChoice ? buildChoices(q, saved, rev) : buildTextarea(q, saved);

    let feedback = '';
    if (rev && !isChoice) {
        const userAns = saved?.freeText || 'Not answered';
        feedback = `
            <div class="feedback show">
                <strong>Your answer:</strong> ${escapeHtml(userAns)}<br>
                <strong>Model answer:</strong> ${escapeHtml(rev.correctAnswer || 'N/A')}<br>
                ${rev.explanation ? `<span class="explanation">${escapeHtml(rev.explanation)}</span>` : ''}
            </div>`;
    }

    return `
        <div class="question-card" data-qid="${q.id}">
            <div class="q-header">
                <span class="q-badge">Question ${index + 1}</span>
                <span class="q-type">${escapeHtml(TYPE_LABELS[q.type] ?? q.type)}</span>
            </div>
            <div class="q-text">${escapeHtml(q.question)}</div>
            ${body}
            ${feedback}
        </div>`;
}

function buildChoices(q, saved, rev) {
    const originalOpts = q.type === 'tf' ? ['True', 'False'] : (q.options || []);

    // الترتيب بيتحدد مرة واحدة لكل سؤال ويتحفظ، عشان الحروف (A, B, C) متتغيرش بعد التسليم.
    // الخلط للـ MCQ بس (True/False يفضل True الأول).
    let order = state.order[q.id];
    if (!order) {
        order = originalOpts.map((_, idx) => idx);
        if (q.type === 'mcq') order = shuffleArray(order);
        state.order[q.id] = order;
    }

    // كل خيار بيحتفظ بالـ index الأصلي
    const displayOpts = order.map(idx => ({ text: originalOpts[idx], originalIndex: idx }));

    const selectedIdx = saved?.selectedOptionIndex;

    const html = displayOpts.map((opt, i) => {
        const isSelected = selectedIdx === opt.originalIndex;
        let cls = 'option-item';

        if (rev) {
            cls += ' disabled-option';
            if (rev.correctAnswer === opt.text) cls += ' correct-answer';
            else if (isSelected) cls += ' wrong-answer';
        } else if (isSelected) {
            cls += ' selected';
        }

        const letter = String.fromCharCode(65 + i);

        // data-opt بياخد الـ originalIndex عشان السيرفر يصحح صح
        return `
            <div class="${cls}" role="radio" aria-checked="${isSelected}" data-opt="${opt.originalIndex}" tabindex="0">
                <span class="option-prefix">${letter}</span>
                <span>${escapeHtml(opt.text)}</span>
            </div>`;
    }).join('');

    const fb = rev ? `
        <div class="feedback ${rev.isCorrect ? 'correct' : 'wrong'}">
            ${rev.explanation ? `<div>${escapeHtml(rev.explanation)}</div>` : ''}
            <div><strong>Correct answer:</strong> ${escapeHtml(rev.correctAnswer || 'N/A')}</div>
        </div>` : '';

    return `<div class="options-list" role="radiogroup">${html}</div>${fb}`;
}

function buildTextarea(q, saved) {
    const val = escapeHtml(saved?.freeText || '');
    const disabled = state.submitted ? 'disabled' : '';
    return `
        <div class="essay-area">
            <textarea id="ta-${q.id}" rows="4" placeholder="Write your answer..." ${disabled}>${val}</textarea>
        </div>`;
}

// Events
function attachEvents() {
    document.querySelectorAll('.options-list').forEach(list => {
        const qid = Number(list.closest('.question-card').dataset.qid);

        list.querySelectorAll('.option-item').forEach(optEl => {
            optEl.addEventListener('click', () => {
                if (state.submitted) return;
                const idx = Number(optEl.dataset.opt);

                state.answers[qid] = { ...state.answers[qid], selectedOptionIndex: idx };

                // visual update
                list.querySelectorAll('.option-item').forEach(el => {
                    el.classList.remove('selected');
                    el.setAttribute('aria-checked', 'false');
                });
                optEl.classList.add('selected');
                optEl.setAttribute('aria-checked', 'true');

                updateProgress();
            });
            // Keyboard accessibility
            optEl.addEventListener('keydown', e => {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); optEl.click(); }
            });
        });
    });

    document.querySelectorAll('textarea').forEach(ta => {
        const qid = Number(ta.id.replace('ta-', ''));
        ta.addEventListener('input', e => {
            if (state.submitted) return;
            state.answers[qid] = { ...state.answers[qid], freeText: e.target.value };
            updateProgress();
        });
    });
}

// Progress
function updateProgress() {
    const total = state.exam.length;
    const answered = state.exam.filter(q => {
        const a = state.answers[q.id];
        return a && (a.selectedOptionIndex != null || (a.freeText && a.freeText.trim()));
    }).length;

    const pct = total ? (answered / total) * 100 : 0;

    $('progressFill').style.width = pct + '%';
    $('progressText').textContent = `${answered} / ${total}`;
}

// Submit
async function handleSubmit() {
    if (state.submitted || !state.exam.length) return;

    const btn = $('submitExamBtn');
    btn.disabled = true;
    btn.textContent = 'Submitting...';

    const payload = state.exam.map(q => ({
        questionId: q.id,
        selectedOptionIndex: state.answers[q.id]?.selectedOptionIndex ?? null,
        freeText: state.answers[q.id]?.freeText ?? null
    }));

    try {
        const result = await submitExamToServer(payload);
        state.submitted = true;

        result.review.forEach(r => state.review[r.questionId] = r);

        renderExam();
        showResult(result);
    } catch (err) {
        console.error(err);
        alert(err.status === 429
            ? 'Too many submissions. Wait a minute and try again.'
            : 'Failed to submit exam. Please try again.');
    } finally {
        // بعد التسليم الزر يفضل مقفول لحد ما يبدأ امتحان جديد
        btn.disabled = state.submitted;
        btn.textContent = 'Submit exam';
    }
}

// Result Modal
function showResult(result) {
    $('scorePercent').textContent = `${result.percent}%`;
    $('scoreMsg').innerHTML =
        `Score: ${Number(result.correctCount)} / ${Number(result.totalGraded)}<br>${escapeHtml(result.feedback)}`;

    const mistakes = result.review.filter(r => !r.isCorrect && r.correctAnswer && r.correctAnswer !== 'N/A');
    const list = $('mistakesList');

    if (!mistakes.length) {
        list.innerHTML = '<p class="perfect">No mistakes. Full score.</p>';
    } else {
        list.innerHTML = `<h4 class="mistakes-title">Mistakes (${mistakes.length})</h4>` +
            mistakes.map(m => `
                <div class="mistake-item">
                    <strong>${escapeHtml(m.question)}</strong><br>
                    Your answer: ${escapeHtml(m.userChoice || 'Not answered')}<br>
                    Correct: ${escapeHtml(m.correctAnswer)}<br>
                    ${m.explanation ? `<span class="explanation">${escapeHtml(m.explanation)}</span>` : ''}
                </div>
            `).join('');
    }

    $('resultModal').style.display = 'flex';
}

// Filters
function renderFilters(categories) {
    // Categories
    const catHtml = ['All', ...categories].map(c => {
        const val = c === 'All' ? 'all' : c;
        const active = state.filters.category === val ? 'active' : '';
        return `<button class="filter-chip ${active}" data-cat="${escapeHtml(val)}">${escapeHtml(c)}</button>`;
    }).join('');
    $('categoryFilters').innerHTML = catHtml;

    // Types
    const types = [
        { v: 'all', l: 'All' },
        { v: 'mcq', l: 'MCQ' },
        { v: 'tf', l: 'True/False' },
        { v: 'essay', l: 'Essay' },
        { v: 'report', l: 'Report' }
    ];
    const typeHtml = types.map(t => {
        const active = state.filters.type === t.v ? 'active' : '';
        return `<button class="filter-chip ${active}" data-type="${t.v}">${t.l}</button>`;
    }).join('');
    $('typeFilters').innerHTML = typeHtml;

    // Listeners
    document.querySelectorAll('[data-cat]').forEach(btn => {
        btn.onclick = () => { state.filters.category = btn.dataset.cat; initExam(); };
    });
    document.querySelectorAll('[data-type]').forEach(btn => {
        btn.onclick = () => { state.filters.type = btn.dataset.type; initExam(); };
    });
}

// Theme
function setTheme(isDark) {
    document.body.classList.toggle('dark', isDark);
    try { localStorage.setItem(THEME_KEY, isDark ? 'dark' : 'light'); } catch { /* private mode */ }
}

function startingTheme() {
    try {
        const saved = localStorage.getItem(THEME_KEY);
        if (saved) return saved === 'dark';
    } catch { /* ignore */ }
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

// Init
function bootstrap() {
    $('newExamBtn').onclick = initExam;
    $('submitExamBtn').onclick = handleSubmit;
    $('resetFiltersBtn').onclick = () => {
        state.filters = { category: 'all', type: 'all' };
        initExam();
    };
    $('closeResultBtn').onclick = () => $('resultModal').style.display = 'none';
    $('newExamFromResultBtn').onclick = () => {
        $('resultModal').style.display = 'none';
        initExam();
    };

    $('lightModeBtn').onclick = () => setTheme(false);
    $('darkModeBtn').onclick = () => setTheme(true);

    document.body.classList.toggle('dark', startingTheme());
    initExam();
}

bootstrap();
