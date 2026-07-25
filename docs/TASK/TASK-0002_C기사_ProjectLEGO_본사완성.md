# Project LEGO

# TASK-0002

## 업무지시서

| 항목 | 내용 |
|---|---|
| 문서번호 | TASK-0002 |
| 제목 | C기사 로컬 GitHub 인증 기반 Project LEGO 본사 완성 |
| 발행일 | 2026-07-25 |
| 발행자 | G팀장 |
| 승인권자 | Y대표 |
| 담당자 | C기사 (Codex) |
| 연계문서 | TASK-0001 Project LEGO 본사 설립 |
| 대상 저장소 | `BBOLOGI/ProjectLEGO` |
| 공개 범위 | Private |
| 기본 브랜치 | `main` |
| 우선순위 | ★★★★★ (최우선) |
| 상태 | 발행 |

---

# 1. 업무 목적

Y대표의 컴퓨터에 이미 로그인되어 있는 GitHub 인증 환경을 사용하여
`BBOLOGI/ProjectLEGO` 저장소의 미완성 상태를 점검하고, Project LEGO의
공식 본사 구조·문서·C#/.NET 솔루션·자동 빌드 체계를 완성한다.

이번 작업은 새 프로그램의 기능 개발이 아니다. 앞으로 모든 Project LEGO
업무가 모이는 공식 본사를 안전하게 완성하는 작업이다.

---

# 2. 현재 인계 상태

W과장이 수행한 작업은 다음과 같다.

- GitHub 비공개 저장소 `BBOLOGI/ProjectLEGO` 생성
- W과장 임시 작업공간에서 기본 문서와 C#/.NET 8 솔루션 작성
- W과장 임시 작업공간에서 로컬 커밋 생성
  - 참고 커밋: `cb8556f`
  - 메시지: `chore: establish Project LEGO headquarters`
- GitHub 웹에서 루트 파일 8개 업로드 및 커밋 시도
- ChatGPT GitHub 연결의 비공개 저장소 접근 제한으로 후속 작업 중단

주의:

- `cb8556f`는 W과장의 임시 로컬 커밋이므로 원격 저장소에 같은 SHA가 없을 수 있다.
- 원격 저장소에는 루트 파일 일부만 존재할 수 있다.
- `docs/`, `src/`, `tests/`, `.github/`, `assets/`, `tools/`는 누락되었을
  가능성이 높다.
- W과장의 임시 작업공간에 의존하지 말고, 원격 저장소의 실제 상태를 기준으로
  작업한다.

---

# 3. 권한 및 보안 원칙

1. Y대표 컴퓨터에 이미 설정된 GitHub 로그인 정보만 사용한다.
2. 비밀번호, Personal Access Token, SSH 개인키, 인증코드를 출력하거나
   문서·로그·소스코드에 기록하지 않는다.
3. 인증이 필요하면 Y대표가 직접 GitHub 공식 로그인 화면에서 수행하게 한다.
4. 기존 원격 이력을 삭제하거나 강제 덮어쓰기하지 않는다.
5. `git push --force`, `git reset --hard`, 원격 저장소 삭제, 브랜치 삭제는 금지한다.
6. 기존 파일이 있으면 먼저 읽고 비교한 후 보존·수정한다.
7. 작업과 관계없는 로컬 파일과 저장소는 수정하지 않는다.

---

# 4. 목표 저장소 구조

```text
ProjectLEGO/
├── .github/
│   └── workflows/
│       └── build.yml
├── .gitignore
├── 00_PROJECT_CHARTER.md
├── README.md
├── CHANGELOG.md
├── LICENSE
├── Directory.Build.props
├── global.json
├── ProjectLEGO.sln
├── docs/
│   ├── tasks/
│   │   ├── TASK-0001_ProjectLEGO_본사설립.md
│   │   └── TASK-0002_C기사_ProjectLEGO_본사완성.md
│   ├── specs/
│   │   └── README.md
│   ├── decisions/
│   │   └── README.md
│   ├── tests/
│   │   └── README.md
│   └── releases/
│       └── README.md
├── src/
│   └── ProjectLEGO.Core/
│       ├── ProjectLEGO.Core.csproj
│       └── ProjectIdentity.cs
├── tests/
│   └── ProjectLEGO.Core.Tests/
│       ├── ProjectLEGO.Core.Tests.csproj
│       └── ProjectIdentityTests.cs
├── assets/
│   └── README.md
└── tools/
    └── README.md
```

Git은 빈 폴더를 관리하지 않으므로 각 기본 폴더에 `README.md`를 두어 구조를
보존한다.

---

# 5. 작업 절차

## 5.1 작업 위치 확정

Y대표에게 Project LEGO 본사를 둘 로컬 상위 폴더를 확인한다.

권장 예:

```text
D:\ProjectLEGO
```

경로를 임의로 결정하지 않는다. 선택된 경로가 기존 파일을 포함하고 있다면
덮어쓰기 전에 반드시 상태를 조사한다.

## 5.2 개발도구 및 로그인 상태 점검

PowerShell에서 다음을 실행한다.

```powershell
git --version
dotnet --info
gh --version
gh auth status
git config --global user.name
git config --global user.email
```

판단 기준:

- `git`과 .NET SDK가 없으면 작업을 중단하고 필요한 설치 항목을 보고한다.
- `gh`가 없어도 Git Credential Manager가 설정되어 있으면 Git 작업은 가능하다.
- `gh auth status`가 성공하면 아래 명령으로 저장소 접근을 확인한다.

```powershell
gh repo view BBOLOGI/ProjectLEGO
```

- 인증 정보 자체는 화면이나 보고서에 복사하지 않는다.
- 인증 실패 시 Y대표가 직접 `gh auth login` 또는 GitHub 공식 브라우저 로그인
  절차를 수행하도록 요청하고, 완료 후 다시 확인한다.

## 5.3 원격 저장소 확보

대상 로컬 폴더가 아직 없다면:

```powershell
git clone https://github.com/BBOLOGI/ProjectLEGO.git
Set-Location ProjectLEGO
```

이미 정상적인 로컬 저장소가 있다면:

```powershell
Set-Location <확정된 ProjectLEGO 경로>
git status --short --branch
git remote -v
git branch --show-current
git fetch origin
```

다음을 확인한다.

- `origin`이 정확히 `https://github.com/BBOLOGI/ProjectLEGO.git`인지
- 현재 브랜치가 `main`인지
- 커밋되지 않은 사용자 변경사항이 있는지
- 원격과 로컬 이력이 갈라져 있는지

사용자 변경사항 또는 이력 충돌이 있으면 임의로 해결하지 말고 Y대표에게
상태를 보고한 후 승인을 받는다.

## 5.4 원격 현재 상태 기록

수정 전에 다음을 확인하고 작업 보고에 남긴다.

```powershell
git status --short --branch
git log --oneline --decorate -10
git ls-tree -r --name-only HEAD
```

루트 파일 일부가 이미 있으면 삭제 후 재작성하지 말고 내용과 목적을 검토하여
필요한 부분만 수정한다.

## 5.5 기본 문서 완성

다음 문서를 작성하거나 보완한다.

### `00_PROJECT_CHARTER.md`

다음 내용을 포함한다.

- Project LEGO는 토목설계 업무의 전산화·자동화를 위한 장기 프로젝트다.
- 검증된 기능을 레고 블록처럼 축적하고 조립한다.
- 이 저장소를 공식 기준점(Single Source of Truth)으로 사용한다.
- 운영 원칙: 정확성, 추적성, 모듈성, 검증성, 보존성, 보안성
- 역할: Y대표, G팀장, W과장, C기사
- 주 대상: ZWCAD 자동화, Excel 자동화, 토목설계 계산 도구

### `README.md`

다음 내용을 포함한다.

- 프로젝트 목적
- 현재 상태
- 저장소 구조
- 필요한 개발환경
- 빌드 및 시험 명령
- 문서 관리 위치
- 비밀정보 및 고객자료 커밋 금지 안내

### `CHANGELOG.md`

`Unreleased` 항목에 본사 기본 구조, 헌장, 솔루션, 시험 및 자동 빌드 추가
내용을 기록한다.

### `LICENSE`

사내용 비공개 저작권 문구를 사용한다. 오픈소스 라이선스를 임의로 적용하지 않는다.

### 업무지시서

- `TASK-0001`을 `docs/tasks/`에 등록한다.
- 이 문서 `TASK-0002`도 `docs/tasks/`에 등록한다.
- TASK-0001 상태는 모든 완료 조건을 검증한 뒤에만 `완료`로 바꾼다.

## 5.6 C#/.NET 초기 솔루션 구성

기본 기준은 .NET 8이다. 설치된 SDK와 호환성을 먼저 확인한다.

필요한 구성:

- 솔루션: `ProjectLEGO.sln`
- 클래스 라이브러리: `src/ProjectLEGO.Core`
- xUnit 시험 프로젝트: `tests/ProjectLEGO.Core.Tests`
- 시험 프로젝트에서 Core 프로젝트 참조
- Nullable 활성화
- 경고를 오류로 처리
- 결정적 빌드 활성화

예시 명령:

```powershell
dotnet new sln --name ProjectLEGO
dotnet new classlib --name ProjectLEGO.Core --output src/ProjectLEGO.Core --framework net8.0
dotnet new xunit --name ProjectLEGO.Core.Tests --output tests/ProjectLEGO.Core.Tests --framework net8.0
dotnet sln ProjectLEGO.sln add src/ProjectLEGO.Core/ProjectLEGO.Core.csproj
dotnet sln ProjectLEGO.sln add tests/ProjectLEGO.Core.Tests/ProjectLEGO.Core.Tests.csproj
dotnet add tests/ProjectLEGO.Core.Tests/ProjectLEGO.Core.Tests.csproj reference src/ProjectLEGO.Core/ProjectLEGO.Core.csproj
```

이미 같은 파일이 있으면 위 명령으로 덮어쓰지 말고 기존 구성을 검토·보완한다.

초기 시험은 최소한 다음을 확인한다.

```text
ProjectIdentity.Name == "Project LEGO"
```

## 5.7 `.gitignore` 작성

다음을 제외하도록 구성한다.

- `bin/`, `obj/`, `Debug/`, `Release/`
- `.vs/`, 사용자별 Visual Studio 설정
- 시험 결과와 coverage 파일
- NuGet 산출물
- 로그 및 임시 파일
- `.env`, 로컬 설정, 비밀정보
- Windows 및 macOS 불필요 파일

소스코드, 솔루션, 공식 문서가 잘못 제외되지 않았는지 확인한다.

## 5.8 GitHub Actions 자동 빌드

`.github/workflows/build.yml`을 작성한다.

요구사항:

- `main` push 및 pull request에서 실행
- 공식 `actions/checkout` 사용
- 공식 `actions/setup-dotnet` 사용
- Restore → Release Build → Test 순서
- 최소 권한 `contents: read`

## 5.9 로컬 검증

다음을 순서대로 실행한다.

```powershell
dotnet restore ProjectLEGO.sln
dotnet build ProjectLEGO.sln --configuration Release --no-restore
dotnet test ProjectLEGO.sln --configuration Release --no-build
git status --short
```

완료 기준:

- Restore 성공
- Build 오류 0개
- Test 전체 통과
- `bin/`, `obj/` 등이 Git 추적 대상에 포함되지 않음
- 목표 구조의 모든 파일 존재
- 비밀번호, 토큰, 고객 기밀자료가 없음

빌드 또는 시험이 실패하면 실패 원인을 수정하고 다시 검증한다. 실패 상태로
커밋·푸시하지 않는다.

## 5.10 커밋 및 푸시

변경 내용을 먼저 확인한다.

```powershell
git status --short
git diff --check
git diff
```

의도한 파일만 스테이징한다.

```powershell
git add .github .gitignore 00_PROJECT_CHARTER.md README.md CHANGELOG.md LICENSE `
  Directory.Build.props global.json ProjectLEGO.sln docs src tests assets tools
git status --short
```

커밋:

```powershell
git commit -m "chore: complete Project LEGO headquarters"
```

푸시:

```powershell
git push -u origin main
```

강제 푸시는 금지한다.

## 5.11 원격 및 자동 빌드 확인

```powershell
gh repo view BBOLOGI/ProjectLEGO --web
gh run list --repo BBOLOGI/ProjectLEGO --limit 5
```

가장 최근 workflow가 완료될 때까지 확인하고, 실패 시 로그를 조사하여 원인을
수정한다.

```powershell
gh run view <RUN_ID> --repo BBOLOGI/ProjectLEGO
gh run view <RUN_ID> --repo BBOLOGI/ProjectLEGO --log-failed
```

최종적으로 GitHub에서 다음을 확인한다.

- 저장소가 Private인지
- 기본 브랜치가 `main`인지
- 목표 폴더와 파일이 모두 표시되는지
- Project Charter와 README가 정상적으로 열리는지
- 최신 Actions Build가 성공했는지

---

# 6. 금지사항

- 새 저장소를 다시 만들지 않는다.
- `BBOLOGI/ProjectLEGO` 이외의 저장소를 수정하지 않는다.
- 저장소 공개 범위를 Public으로 변경하지 않는다.
- GitHub 로그인 정보나 토큰을 요청·복사·저장하지 않는다.
- 기존 원격 커밋을 삭제하거나 이력을 강제로 재작성하지 않는다.
- 빌드 실패를 숨기거나 Build Success로 보고하지 않는다.
- 빈 폴더만 만들고 완료로 보고하지 않는다.
- 대표의 승인 없이 기능 개발, 외부 라이브러리 추가 또는 기술 범위 확장을 하지 않는다.

---

# 7. 완료 조건

아래 항목을 모두 충족해야 TASK-0001과 TASK-0002를 완료로 보고할 수 있다.

- [ ] `BBOLOGI/ProjectLEGO` 접근 및 원격 상태 확인
- [ ] 저장소 Private 확인
- [ ] 기본 브랜치 `main` 확인
- [ ] 목표 폴더·문서 구조 등록
- [ ] `00_PROJECT_CHARTER.md` 등록
- [ ] `README.md`, `CHANGELOG.md`, `LICENSE` 등록
- [ ] `.gitignore` 등록
- [ ] `ProjectLEGO.sln`과 Core/Test 프로젝트 등록
- [ ] 로컬 Restore 성공
- [ ] 로컬 Release Build 성공
- [ ] 전체 Test 통과
- [ ] GitHub Actions 최신 Build 성공
- [ ] 커밋 및 `origin/main` 푸시 완료
- [ ] 원격 저장소에서 최종 파일 확인

---

# 8. C기사 최종 보고 형식

```markdown
# TASK-0002 완료 보고

## 1. 저장소
- URL:
- 공개 범위:
- 기본 브랜치:

## 2. 작업 결과
- 생성·수정한 주요 파일:
- 최종 커밋 SHA:
- 커밋 메시지:

## 3. 검증 결과
- dotnet restore:
- Release build:
- tests:
- GitHub Actions:

## 4. 기존 원격 상태 처리
- 작업 전 원격 상태:
- 보존 또는 수정한 기존 파일:
- 충돌 여부:

## 5. 남은 사항
- 없음 / 상세 내용
```

보고에는 비밀번호, 토큰, 인증 URL 및 개인 인증정보를 포함하지 않는다.

---

# G팀장 지시

C기사는 Y대표 컴퓨터에 설정된 정상적인 GitHub 인증을 이용하여 작업하되,
원격 저장소의 기존 상태를 먼저 확인하고 보존한다. 본사는 구조만 갖춘 것으로
완료되지 않는다. 로컬 빌드와 시험, GitHub Actions 성공까지 확인한 뒤 완료를
보고한다.
