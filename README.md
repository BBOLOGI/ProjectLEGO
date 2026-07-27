# Project LEGO

Project LEGO는 철골설계 업무의 계산, 자동화, 문서화를 위한 장기 프로젝트입니다.
검증된 기능을 블록처럼 축적하고 조립하는 것을 목표로 합니다.

## 현재 상태

본사 기본 구조와 문서, .NET 8 Core 라이브러리, xUnit 테스트 및 GitHub Actions
빌드 체계와 JSON 기반 LEGO Template Engine을 구성한 초기 단계입니다.

## 저장소 구조

- `.github/workflows`: CI 워크플로
- `docs/TASK`: 작업지시서
- `docs/SPEC`: 기능 명세
- `docs/DECISION`: 설계 결정
- `docs/TEST`: 시험 문서
- `docs/RELEASE`: 릴리스 문서
- `src/ProjectLEGO.Core`: 핵심 라이브러리
- `src/ProjectLEGO.Desktop`: LEGO 검색 WinForms 앱
- `tests/ProjectLEGO.Core.Tests`: 자동 시험
- `data/templates`: LEGO Template JSON
- `data/legos`: 등록 LEGO Record JSON
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

## LEGO Template Engine

Template는 `data/templates` 아래의 JSON 파일로 관리합니다. 엔진은 하위 폴더를
포함한 모든 `*.json` 파일을 읽고 각 파일을 독립적으로 검증하므로, 잘못된 파일
하나가 다른 정상 Template의 로드를 막지 않습니다.

새 Template를 추가하려면 기존 샘플의 스키마를 따라 고유한 `TemplateId`를 가진
UTF-8 JSON 파일을 `data/templates` 또는 그 하위 폴더에 저장한 뒤 Manager를
다시 로드합니다. 소스코드 수정이나 재빌드는 필요하지 않습니다.

```csharp
var manager = new TemplateManager("data/templates");
var result = manager.LoadAll();
var template = manager.GetTemplate("catch-basin");
```

지원 형식은 `Single`, `Standard`, `Conditional`이며, 필드 형식은 `Number`,
`Text`, `Boolean`, `Select`입니다.

## LEGO 검색 Desktop UI v2.0

```powershell
dotnet run --project src/ProjectLEGO.Desktop/ProjectLEGO.Desktop.csproj --configuration Release
```

앱은 Repository와 SearchService 계층을 통해 `data/templates`와 `data/legos`의
LEGO를 통합 검색합니다. 이름·설명·태그·규격에 대한 부분 검색을 지원하고,
공백과 `×/x/X` 표기를 정규화합니다.

결과를 선택하면 Template의 검색 가능 필드가 규격1~4 계층으로 표시됩니다.
상위 규격을 변경하면 하위 선택이 초기화되며, 최종 LEGO가 유일하게 결정되어야
Preview와 열기·삽입 Mock 버튼이 활성화됩니다. 실제 CAD 삽입은 포함하지 않습니다.

## 문서 관리

Project LEGO의 운영 문서는 아래 위치에서 관리합니다.

## Project Documents

- Constitution: `docs/CONSTITUTION/PROJECT_LEGO_헌법.md`
- Standards: `docs/STANDARDS/`
- Tasks: `docs/TASK/`
- Specifications: `docs/SPEC/`
- Decisions: `docs/DECISION/`
- Tests: `docs/TEST/`
- Releases: `docs/RELEASE/`

## 보안

비밀번호, 토큰, 개인 인증정보 및 고객 기밀자료는 커밋하지 않습니다.
로컬 환경값은 `.env` 등 Git에서 제외된 파일로만 관리합니다.
