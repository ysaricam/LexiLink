# LexiLink Frontend

Flutter client for iOS, Android, and web.

Reference model:

- Flutter official architecture
- Very Good Ventures project discipline
- Bloc/Cubit for state management

## Bootstrap

Flutter is required locally.

```bash
cd frontend
flutter pub get
flutter analyze
flutter test
flutter run -d chrome
```

If platform folders are missing after a manual bootstrap, generate them from
inside `frontend/`:

```bash
flutter create . --platforms=ios,android,web
```
