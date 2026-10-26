// api.js
// Frontend و Backend على نفس السيرفر، فمفيش داعي نكتب localhost

const API_BASE_URL = '';

export class ApiError extends Error {
  constructor(status, message) {
    super(message);
    this.status = status;
  }
}

/**
 * جلب الأسئلة من الـ API مع إمكانية الفلترة
 */
export async function fetchQuestions(category = 'all', type = 'all') {
  const params = new URLSearchParams();
  if (category && category !== 'all') params.set('category', category);
  if (type && type !== 'all') params.set('type', type);

  const query = params.toString();

  // استخدمنا متغير منفصل عشان نمنع الـ Nested Template Literals
  const queryString = query ? `?${query}` : '';
  const url = `${API_BASE_URL}/api/questions${queryString}`;

  const res = await fetch(url, { cache: 'no-store' });
  if (!res.ok) throw new ApiError(res.status, `HTTP ${res.status} while fetching questions`);
  const data = await res.json();
  return data.questions || [];
}

/**
 * جلب قائمة التصنيفات
 */
export async function fetchCategories() {
  const res = await fetch(`${API_BASE_URL}/api/questions/categories`);
  if (!res.ok) throw new ApiError(res.status, `HTTP ${res.status} while fetching categories`);
  return await res.json();
}

/**
 * إرسال الامتحان للتصحيح
 * @param {Array} answers - [{ questionId, selectedOptionIndex, freeText }]
 */
export async function submitExamToServer(answers) {
  const res = await fetch(`${API_BASE_URL}/api/exams/evaluate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ answers }),
  });
  if (!res.ok) throw new ApiError(res.status, `HTTP ${res.status} while submitting exam`);
  return await res.json();
}
