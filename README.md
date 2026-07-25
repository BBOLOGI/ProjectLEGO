# Project LEGO

Project LEGO는 철골설계 업무의 계산, 자동화, 문서화를 위한 장기 프로젝트입니다.
검증된 기능을 블록처럼 축적하고 조립하는 것을 목표로 합니다.

## 현재 상태

본사 기본 구조와 문서, .NET 8 Core 라이브러리, xUnit 테스트 및 GitHub Actions
빌드 체계를 구성한 초기 단계입니다.

## 저장소 구조

- `.github/workflows`: CI 워크플로
- `docs/tasks`: 작업지시서
- `docs/specs`: 기능 명세
- `docs/decisions`: 설계 결정
- `docs/tests`: 시험 문서
- `docs/releases`: 릴리스 문서
- `src/ProjectLEGO.Core`: 핵심 라이브러리
- `tests/ProjectLEGO.Core.Tests`: 자동 시험
- `assets`: 프로젝트 자산 안내
- `tools`: 개발 지원 도구 안내

## 개발 환경

- .NET SDK 8.x
- Git

## 빌드 및 시험

```powershell
dotnet restore ProjectLEGO.sln
dotnet build ProjectLEGO.sln --configuration Release --no-restore
dotnet test ProjectLEGO.sln --configuration Release --no-build
```

## 문서 관리

업무 지시는 `docs/tasks`, 명세는 `docs/specs`, 결정 기록은 `docs/decisions`,
시험 자료는 `docs/tests`, 릴리스 기록은 `docs/releases`에서 관리합니다.

## 보안

비밀번호, 토큰, 개인 인증정보 및 고객 기밀자료는 커밋하지 않습니다.
로컬 환경값은 `.env` 등 Git에서 제외된 파일로만 관리합니다.

