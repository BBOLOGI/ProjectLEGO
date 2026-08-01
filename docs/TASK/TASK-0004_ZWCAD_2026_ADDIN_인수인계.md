# TASK-0004 Project LEGO ZWCAD 2026 Add-in 인수인계

## 1. 문서 목적

이 문서는 집에서 Y대표와 C기사(Codex)가 논의한 Project LEGO의 최신 방향,
현재 저장소 상태, UI 결과물, 향후 구현 기준을 사무실 컴퓨터의 Codex에 전달하기 위한
인수인계 문서다.

사무실에서는 과거 TASK-0004 문서만 보고 기존 검색 UI를 다시 구현하지 말고,
이 문서를 최신 사용자 요구사항으로 우선 적용한다.

---

## 2. 저장소와 Git 상태

- 로컬 저장소: `D:\Coding\ProjectLEGO`
- 원격 저장소: `https://github.com/BBOLOGI/ProjectLEGO`
- 현재 브랜치: `feature/task-0004-search-ui-v2`
- 원격 브랜치: `origin/feature/task-0004-search-ui-v2`
- 원격에 Push된 최신 Commit: `a3b03194a1b1fc4c9053ff2c942a73c4c262377c`
- Commit 메시지: `fix: align search results with hierarchical selection`

이번 인수인계 Commit에 포함할 집 컴퓨터의 UI 변경:

```text
M src/ProjectLEGO.Desktop/MainForm.Designer.cs
M src/ProjectLEGO.Desktop/MainForm.cs
```

위 두 파일은 기능을 제거하고 새 실무형 UI 골격으로 변경한 결과다.
본 문서와 함께 `feature/task-0004-search-ui-v2` 브랜치에 Commit·Push하여 전달한다.
사무실에서는 해당 브랜치를 Pull한다.

---

## 3. 기존 구현 이력

### TASK-0002

- LEGO Template Engine 구현
- Domain Model, JSON Serializer, Template Loader, Validator, Manager, Sample, Unit Test 구현
- Commit: `36e33fb feat: implement LEGO Template Engine`

### TASK-0003

- LEGO 검색과 WinForms 기본 UI 구현
- Commit: `e8a43ab feat: implement LEGO search and desktop UI`

### TASK-0004 초기 구현

- 통합 검색 UI v2.0 구현
- Repository → SearchService → UI 구조 적용
- Release Build 및 테스트 통과
- Commit: `1111aae feat: implement Project LEGO search UI v2`

### TASK-0004 보완

- 빈 검색어 결과 0건 처리
- 검색 결과를 LEGO 종류 단위로 묶음
- Commit: `a3b0319 fix: align search results with hierarchical selection`

이후 Y대표가 기존 화면을 실무 흐름에 맞지 않는 것으로 판단하여 방향을 다시 정의했다.

---

## 4. 최신 제품 방향

Project LEGO는 독립 실행형 EXE가 아니라 **ZWCAD 2026에서 실행되는 .NET Add-in**으로 만든다.

```text
ProjectLEGO.Addin.dll
→ ZWCAD 2026에서 로드
→ ZWCAD 명령으로 Project LEGO 창 표시
```

현재 `ProjectLEGO.Desktop.exe`는 UI를 확인하기 위한 임시 호스트일 뿐이다.
최종 제품 형태로 간주하지 않는다.

핵심 로직은 ZWCAD API와 분리한다.

```text
ProjectLEGO.Core
├─ 등록 정보 모델
├─ 검색과 계층 선택
├─ Repository
└─ 저장 경로 설정

ProjectLEGO.Addin
├─ ZWCAD 명령
├─ WinForms UI
├─ 선택 객체 DWG 저장
├─ DWG 열기
└─ 현재 도면 삽입
```

ZWCAD 2026이 요구하는 .NET 런타임, x64 설정, ZRX.NET 참조 DLL은
사무실의 실제 설치 환경을 먼저 조사한 뒤 결정한다. 현재 `.NET 8`을 그대로 사용한다고
가정하지 않는다.

---

## 5. UI 확정안

메인 화면은 WinForms `TabControl`의 두 탭으로 구성한다.

1. 등록 탭
2. 호출 탭

현재 집 컴퓨터의 `MainForm.Designer.cs`에 이 UI 골격이 작성되어 있다.
기능 이벤트는 연결하지 않았다.

### 5.1 등록 탭

```text
LEGO 이름   [____________________________]

LEGO 특성1  [____________________________]
LEGO 특성2  [____________________________]
LEGO 특성3  [____________________________]
LEGO 특성4  [____________________________]

[등록] [닫기]
```

- 등록 탭의 `수정` 버튼은 제거한다.
- LEGO 이름은 필수다.
- 특성1~4는 없는 값부터 비워 둘 수 있다.
- 특성은 중간을 건너뛰지 않고 왼쪽부터 입력하는 것을 기본 등록 규칙으로 삼는다.

### 5.2 호출 탭

호출 탭에는 ListBox 다섯 개를 한 화면에 가로로 배치한다.

```text
LEGO 이름 검색 [____________________________] [검색]

┌────────────┬────────────┬────────────┬────────────┬────────────┐
│ 검색 결과  │ 특성1      │ 특성2      │ 특성3      │ 특성4      │
├────────────┼────────────┼────────────┼────────────┼────────────┤
│ 집수정     │ 현장타설   │ 500x400... │ H=1.0     │ 콘크리트   │
│ 집수거     │ 기성품     │ 600x500... │ H=1.5     │ PE         │
│ 집수암거   │            │            │            │            │
└────────────┴────────────┴────────────┴────────────┴────────────┘

선택된 LEGO: 집수정 > 현장타설 > 500x400x1000 > H=1.5 > 콘크리트

[열기] [삽입] [수정] [닫기]
```

창 크기를 변경해도 다섯 ListBox는 같은 비율로 늘어나도록 `TableLayoutPanel` 5열을 사용한다.

---

## 6. 호출 ListBox 동작 규칙

### 6.1 이름 검색

검색어 `집수`를 입력하면 이름에 해당 문자열이 포함된 LEGO 이름을 중복 없이 표시한다.

```text
집수정
집수거
집수암거
```

- 대소문자와 공백 차이는 검색 정규화에서 처리한다.
- 검색 전에는 결과를 표시하지 않는다.
- 빈 검색어를 전체 검색으로 취급하지 않는다.

### 6.2 선택 목록 유지

`집수정`을 선택해도 검색 결과 목록은 그대로 유지한다.

```text
▶ 집수정
  집수거
  집수암거
```

선택 항목은 포커스가 다른 ListBox로 이동해도 분명히 보이도록 처리한다.

- 선택 배경색 유지
- 선택 글자색 변경
- 굵은 글씨 적용
- WinForms `OwnerDrawFixed` 사용 가능

### 6.3 단계별 특성 선택

```text
검색 결과에서 집수정 선택
→ 특성1에 현장타설 / 기성품 표시

특성1에서 현장타설 선택
→ 특성2에 해당 조건의 값 표시

특성2 선택
→ 특성3 표시

특성3 선택
→ 특성4 표시
```

각 ListBox는 선택 후 목록을 지우거나 선택 항목 하나만 남기지 않는다.
전체 후보를 유지하고 현재 선택만 강조한다.

### 6.4 상위 선택 변경

상위 항목을 바꾸면 오른쪽 목록만 초기화하고 다시 조회한다.

```text
특성1 변경
→ 특성1 목록과 새 선택은 유지
→ 특성2를 새 조건으로 다시 생성
→ 특성3 초기화
→ 특성4 초기화
```

검색 결과에서 다른 LEGO 이름을 선택하면:

```text
검색 결과 목록 유지
→ 새 LEGO 이름 강조
→ 특성1 새로 생성
→ 특성2~4 초기화
```

### 6.5 최종 선택 판정

버튼 활성화 기준은 `특성4까지 선택했는가`가 아니다.

> 현재 선택 조건과 일치하는 실제 LEGO 파일이 정확히 하나인가?

예를 들어 특성1만 등록된 LEGO는:

```text
집수정 선택
→ 특성1에서 현장타설 선택
→ 일치하는 파일 1개
→ 열기 / 삽입 / 수정 활성화
```

이름만 있고 특성이 없는 LEGO는 이름 선택만으로 파일이 하나로 확정되면 즉시 버튼을 활성화한다.

```text
이름만 등록됨       → 이름 선택 후 1개 확정
특성1까지 등록됨    → 특성1 선택 후 1개 확정
특성2까지 등록됨    → 특성2 선택 후 1개 확정
특성3까지 등록됨    → 특성3 선택 후 1개 확정
특성4까지 등록됨    → 필요한 선택 후 1개 확정
```

일치 파일이 0개이거나 2개 이상이면 `열기`, `삽입`, `수정`은 비활성화한다.

---

## 7. 등록 버튼 동작 확정안

등록 버튼은 ZWCAD에서 선택한 객체를 독립된 LEGO DWG와 메타데이터로 저장한다.

```text
등록 정보 입력
→ 등록 버튼
→ ZWCAD에서 객체 선택
→ 삽입 기준점 지정
→ 독립 DWG 생성
→ metadata.json 저장
→ 호출 검색 목록 갱신
```

### 7.1 입력 검증

- LEGO 이름 필수
- 특성1~4 빈 값 허용
- 경로 접근 확인
- 동일한 이름 + 특성1~4 조합의 중복 확인
- 중복은 자동 덮어쓰기하지 않고 등록 거부

### 7.2 ZWCAD 선택

등록 버튼 클릭 후 ZWCAD 명령줄에 다음 흐름을 제공한다.

```text
LEGO로 등록할 객체를 선택하십시오.
LEGO 삽입 기준점을 지정하십시오.
```

선택 취소 또는 기준점 지정 취소 시 아무 파일도 등록하지 않는다.

### 7.3 저장 결과

각 LEGO는 고유 ID 폴더에 저장한다.

```text
Z:\DATA\LEGO\records\LEGO-20260801-0001\
├─ lego.dwg
└─ metadata.json
```

메타데이터 예시:

```json
{
  "Id": "LEGO-20260801-0001",
  "Name": "집수정",
  "Feature1": "현장타설",
  "Feature2": "500x400x1000",
  "Feature3": "H=1.5",
  "Feature4": "콘크리트",
  "DrawingFile": "lego.dwg",
  "CreatedAt": "2026-08-01T10:30:00+09:00"
}
```

DWG와 메타데이터 중 하나라도 저장에 실패하면 등록을 완료하지 않는다.
부분 생성 파일을 정리하고 오류를 안내한다.

---

## 8. 호출 탭 버튼 동작

### 8.1 열기

- 최종 선택된 `lego.dwg`를 ZWCAD 문서로 실제 연다.
- 독립 EXE나 안내 MessageBox만 표시하는 Mock으로 끝내지 않는다.

### 8.2 삽입

- 최종 선택된 `lego.dwg`를 현재 ZWCAD 도면에 실제 삽입한다.
- 등록 시 지정한 기준점을 삽입 기준으로 사용한다.
- 구체적인 블록 삽입/외부 DWG 처리 방식은 ZWCAD 2026 API 검증 후 확정한다.

### 8.3 수정

호출 탭의 `수정`은 이름이나 규격값 편집 기능이 아니다.

> 선택된 LEGO의 특성1~4 값 위치를 재배열하는 기능이다.

사용자가 반복 등록 과정에서 특성 위치를 혼동했을 때 사용한다.

예:

```text
변경 전
특성1: 500x500
특성2: 현장타설
특성3: 콘크리트
특성4: H=1.5

변경 후
특성1: 현장타설
특성2: 500x500
특성3: H=1.5
특성4: 콘크리트
```

별도 `규격 위치 수정` 대화상자를 사용한다.

- 현재 네 값을 각 위치에서 다시 선택
- 값 내용은 바꾸지 않음
- 같은 값을 중복 배치하지 않음
- 변경 전/후 표시
- 저장/취소 제공
- 저장 시 `metadata.json`만 변경
- `lego.dwg`는 변경하지 않음
- 저장 후 호출 ListBox 새로고침

### 8.4 닫기

- Project LEGO UI를 닫는다.
- ZWCAD 자체는 종료하지 않는다.

---

## 9. 기본 저장 경로와 설정

등록과 호출에 사용할 기본 폴더:

```text
Z:\DATA\LEGO
```

경로를 UI, Repository, ZWCAD 명령에 각각 하드코딩하지 않는다.

```text
등록 UI ─┐
         ├→ LegoStorageOptions → Repository → Z:\DATA\LEGO
호출 UI ─┘
```

외부 설정 예시:

```json
{
  "LegoRootPath": "Z:\\DATA\\LEGO"
}
```

향후 설정 화면에서 폴더를 바꿀 수 있도록 여지를 둔다.
현재는 설정 UI가 필수는 아니다.

Z 드라이브에 접근할 수 없을 때 임의의 로컬 폴더로 대체 저장하지 않는다.

```text
LEGO 기본 폴더에 접근할 수 없습니다.
Z:\DATA\LEGO 연결 상태를 확인하십시오.
```

---

## 10. 사무실에서 구현 전 필수 환경 조사

구현을 시작하기 전에 다음을 읽기 전용으로 확인하고 보고한다.

1. ZWCAD 2026 정확한 에디션과 버전
2. ZWCAD 2026 설치 경로
3. ZRX.NET 또는 .NET API 참조 DLL 위치
4. 공식 개발 가이드 및 샘플 프로젝트 존재 여부
5. Add-in이 요구하는 Target Framework
6. x64 빌드 조건
7. DLL 로드 명령과 자동 로드 방식
8. 기존에 개발한 다른 ZWCAD Add-in의 프로젝트 설정
9. 기존 Add-in에서 사용한 참조 DLL과 `Copy Local` 설정
10. `Z:\DATA\LEGO` 읽기·쓰기 가능 여부
11. 테스트용 DWG와 등록할 간단한 객체 준비 여부

기존 Add-in 프로젝트는 좋은 기준 자료지만, 임의로 수정하거나 복사하지 말고 먼저 구조와
참조 설정만 조사한다.

환경 확인 전에는 현재 `.NET 8` Desktop 프로젝트를 ZWCAD Add-in으로 무리하게 전환하지 않는다.

---

## 11. 구현 순서 제안

1. 현재 Git 상태와 이 문서 확인
2. 집에서 만든 UI 변경이 Commit되어 있는지 확인
3. ZWCAD 2026 API/SDK 환경 조사
4. `ProjectLEGO.Core`에 단순 positional LEGO 모델과 Repository 구현
5. 변경 가능한 `LegoStorageOptions` 구현
6. 파일 기반 메타데이터 검색 및 계층 선택 서비스 구현
7. `ProjectLEGO.Addin` Class Library 생성
8. ZWCAD 명령으로 MainForm 표시
9. 등록 객체 선택과 기준점 지정 구현
10. DWG/metadata 원자적 저장 구현
11. 호출 ListBox 5단계 연결
12. 실제 열기 구현
13. 실제 삽입 구현
14. 특성 위치 수정 대화상자 구현
15. Core 단위 테스트
16. Release Build
17. ZWCAD에서 수동 로드 및 실제 DWG 검증
18. 문서, Commit, Push

---

## 12. 구현하지 않거나 아직 확정하지 않은 사항

- SQLite 전환
- NAS 전용 프로토콜
- 이미지 Preview
- DWG Thumbnail
- 즐겨찾기
- 최근 사용
- AI 검색
- 사용자 관리
- 권한 관리
- 태그 검색
- 등록된 DWG 내용 자체를 UI에서 편집하는 기능

위 기능은 이번 구현 범위에 임의로 추가하지 않는다.

다음 사항은 사무실 환경 조사 후 확정한다.

- ZWCAD 2026 Target Framework
- 참조할 ZRX.NET DLL 정확한 파일명
- Add-in 자동 로드 배포 방식
- DWG 삽입 시 블록 이름 충돌 정책
- 단위와 축척 처리
- 레이어/문자스타일/치수스타일 충돌 처리

---

## 13. 주의사항

- 기존 파일과 미커밋 변경을 삭제하거나 덮어쓰지 않는다.
- 집에서 만든 UI는 기능 없는 Designer 골격이다.
- 기존 TASK-0004의 Preview 중심 UI를 다시 복원하지 않는다.
- Dropdown/ComboBox 계층 선택으로 되돌리지 않는다.
- 호출 탭은 반드시 가로 ListBox 5개를 유지한다.
- 최종 선택은 마지막 특성 번호가 아니라 일치 파일 1개 여부로 판정한다.
- 등록 탭에는 수정 버튼을 두지 않는다.
- 호출 탭 수정은 특성 위치 재배열 전용이다.
- ZWCAD API 확인 전 실제 Add-in 코드를 추측으로 작성하지 않는다.

---

## 14. 사무실 Codex 재개 프롬프트

사무실 컴퓨터에서 아래 내용을 그대로 사용할 수 있다.

```text
작업 경로는 D:\Coding\ProjectLEGO다.

먼저 현재 경로, Git 저장소 여부, 브랜치, git status,
로컬과 원격 SHA를 확인하라.

docs/TASK/TASK-0004_ZWCAD_2026_ADDIN_인수인계.md를 처음부터 끝까지 읽고,
기존 TASK-0004보다 이 인수인계 문서의 최신 요구사항을 우선 적용하라.

미커밋 변경사항이 있으면 삭제하거나 덮어쓰지 말고 먼저 보고하라.
이미 완료된 작업은 다시 수행하지 마라.

아직 기능 구현을 시작하지 말고 먼저 다음을 조사하라.
- ZWCAD 2026 설치 위치와 버전
- ZRX.NET/.NET API DLL
- Target Framework와 x64 조건
- 기존 ZWCAD Add-in 프로젝트의 참조 및 로드 설정
- Z:\DATA\LEGO 접근 여부

조사 결과와 실제 구현 가능 환경을 먼저 보고하고 다음 지시를 기다려라.
```

---

## 15. 현재 중단 지점

- 최신 실무 요구사항 논의 완료
- 2개 탭 UI Designer 골격 작성 완료
- Desktop 프로젝트 UI 빌드 성공(경고 0, 오류 0)
- UI 기능 연결하지 않음
- ZWCAD Add-in 전환하지 않음
- ZWCAD API 환경 조사하지 않음
- 집의 UI 변경과 본 인수인계 문서는 인수인계 Commit으로 Push

다음 작업은 **사무실 PC에서 해당 브랜치를 Pull하고 ZWCAD 2026 환경을 읽기 전용으로 조사하는 것**이다.
