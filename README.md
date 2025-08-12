# Crash & Log Starter Kit (WinForms)

![thumbnail](./assets/thumbnail.png)

WinForms 앱에서 **전역 예외를 반드시 다이얼로그로 알리고**, 동시에 **파일 로그**로 남기는 최소 구성 스타터 킷입니다.  
UI/백그라운드 스레드를 모두 커버하며, **의존성 0** (.NET 기본 API)로 동작합니다.

## ✨ 특징
- 전역 예외 캡처 (UI/Non-UI)
- 예외 다이얼로그 강제 표출
- 쓰레드 안전 파일 로그
- 용량 기반 롤링 (기본 5MB × 5개)
- .NET 6 WinForms 프로젝트 템플릿 포함

## 🚀 실행 방법
1) Visual Studio 2022 이상에서 이 폴더 열기(Open Folder) 또는 솔루션 생성 후 `CrashLog-StarterKit.csproj` 추가  
2) 빌드 & 실행 → 버튼으로 UI/백그라운드 예외 테스트  
3) `./logs/app.log` 파일 생성 확인

## 📁 구성
```text
CrashLog-StarterKit/
├─ CrashLog-StarterKit.csproj
├─ src/
│  ├─ Program.cs
│  ├─ ExceptionHandler.cs
│  ├─ RollingFileLogger.cs
│  ├─ Log.cs
│  └─ DemoForm.cs
├─ assets/
│  └─ thumbnail.png
├─ .gitignore
├─ LICENSE
└─ README.md
```

## 🔧 프로젝트에 통합 (1분)
`Program.cs`의 `Main`에 아래 이벤트 등록
```csharp
Application.ThreadException += ExceptionHandler.OnUIThreadException;
AppDomain.CurrentDomain.UnhandledException += ExceptionHandler.OnNonUIThreadException;
```
그리고 `ExceptionHandler.cs`, `RollingFileLogger.cs`, `Log.cs`를 프로젝트에 추가하세요.

## 📝 라이선스
MIT (상업적 사용 가능, 표기 유지)
