# Contributing

Thanks for taking the time. This is a small solo project, so the process is simple.

## Before you start

For anything bigger than a typo or a small fix, open an issue first so we can agree on the approach before you spend time on it. Bug reports should include the steps to reproduce, what you expected, what happened instead, and your OS, browser and .NET version.

Security problems do not go in public issues. See [SECURITY.md](./SECURITY.md).

## Running the project

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
git clone https://github.com/YOUR-USERNAME/my-exam-app.git
cd my-exam-app
dotnet run --launch-profile https
```

The app is at https://localhost:7019 (plain `dotnet run` serves http://localhost:5152). There are no automated tests at the moment, so before opening a pull request please run `dotnet build` and take one exam through the browser: pick answers, submit, and check the review. CI only builds the project.

## Pull requests

- Fork the repo and work on a branch named after what you are changing.
- Keep each pull request to one change.
- In the description, say what changed and how you checked it.
- If you changed the API or added a feature, update the README.

Commit messages should be short and say what changed. Prefixes like `fix:` or `feat:` are fine but not required.

## Code style

Follow the code around your change. A few things that are consistent in the project:

- C#: PascalCase for types and methods, `_camelCase` for private fields.
- JavaScript: ES modules, `const` by default, `let` only when you reassign.
- CSS: colors come from the custom properties at the top of `style.css`, so light and dark theme keep working.
- Anything from the server that ends up in the page goes through `escapeHtml()` first.

## Adding or fixing questions

Questions live in `Data/questions.json`, in the `questions` array. Each one needs a unique `id` (use the next free number), a `category`, a `type` and a `question`. The rest depends on the type:

```json
{ "id": 1, "category": "Grammar", "type": "mcq",
  "question": "Some engineers ____ many design specifications.",
  "options": ["give", "gives", "are give", "is give"],
  "correct": 0,
  "explanation": "The subject is plural, so the plural verb form is correct." }
```

- `mcq`: four `options`, `correct` is the index of the right one (starting at 0), plus an `explanation`.
- `tf`: `options` is `["True", "False"]`, `correct` is 0 or 1, plus an `explanation`.
- `essay`: an `answer` (the model answer). A `rubric` is optional.
- `report`: an `answer` and a `rubric`.

Update `total` at the top of the file if you add or remove questions.

The questions are only loaded into the database when it is empty. After editing the file, stop the app, delete `examapp.db` (and `examapp.db-shm` / `examapp.db-wal` if they exist) and start it again. On startup the seeder checks that every `mcq` and `tf` question has a `correct` index that points at one of its options and that ids are unique. If something is wrong the app stops and the error names the question.

Questions added to this file fall under the terms stated in the README.

## License

Code contributions are licensed under the [MIT License](./LICENSE).
