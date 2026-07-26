# TASK-0003 — LEGO 검색 기능 및 기본 UI 구축

- **Status**: Approved / Ready for Implementation
- **Priority**: High
- **Assignee**: C기사(Codex)
- **Approver**: Y대표
- **Author**: G팀장
- **Project**: Project LEGO
- **Repository**: `BBOLOGI/ProjectLEGO`
- **Local path**: `D:\Coding\ProjectLEGO`
- **Predecessor**: `TASK-0002 — LEGO Template Engine`

---

## 1. 작업 명령

Project LEGO에 등록된 LEGO 블록을 실제로 찾아 사용할 수 있도록 **LEGO 검색 엔진과 기본 데스크톱 UI**를 구현한다.

승인된 사용자 흐름은 다음과 같다.

```text
검색어 입력 → 검색 결과 표시 → Template 기반 상세 필터 → LEGO 선택 → 삽입 요청
```

이번 Task에서는 검색부터 **삽입 요청 이벤트 발생**까지 구현한다. 실제 ZWCAD 도면 삽입은 후속 Task로 분리한다.

C기사는 계획 보고로 종료하지 않는다. 실제 코드, UI, 샘플 데이터, 테스트를 작성하고 Build·Test·실행 확인·Commit·Push까지 수행한다.

---

## 2. 동결된 제품 원칙

### 2.1 Project LEGO는 파일 탐색기가 아니다

사용자가 폴더를 돌아다니며 DWG를 찾게 하지 않는다.

```text
Search → Results → Filters → Insert
```

파일 경로는 시스템 내부 정보이며 검색 결과의 중심 정보가 아니다.

### 2.2 필터 생성 기준

> 서로 다른 LEGO를 구분하는 속성만 필터가 된다.

Template Field의 `Searchable = true`인 항목만 동적 필터로 표시한다.

### 2.3 Template가 UI를 결정한다

구조물별 필터를 코드에 하드코딩하지 않는다.

```text
Template → Searchable Field → 동적 검색 필터 UI
```

새 Template와 LEGO 데이터가 추가되어도 구조물별 분기 코드를 수정하지 않아야 한다.

---

# PART A — 등록 LEGO 데이터 기반

## 3. LEGO Record 모델

검색 대상은 Template 자체가 아니라 **등록된 개별 LEGO Record**이다.

`LegoRecord` 최소 속성:

| 속성 | 형식 | 설명 |
|---|---|---|
| `LegoId` | string | 변경되지 않는 영구 식별자 |
| `TemplateId` | string | 적용 Template ID |
| `Name` | string | 검색 결과 표시명 |
| `Description` | string/null | 설명 |
| `Category` | string | 공종 또는 분류 |
| `Tags` | collection<string> | 동의어와 검색 태그 |
| `Properties` | dictionary | Template Field 값 |
| `AssetPath` | string | 도면 또는 자산의 상대/논리 경로 |
| `ThumbnailPath` | string/null | 기존 썸네일 경로 |
| `IsActive` | bool | 검색 노출 여부 |
| `CreatedAt` | datetime | 등록 시각 |
| `UpdatedAt` | datetime | 수정 시각 |

필수 규칙:

- `LegoId`는 표시 이름·파일명과 분리한다.
- `Properties` Key는 Template Field의 `InternalName`을 사용한다.
- 절대경로를 코드에 하드코딩하지 않는다.
- `IsActive = true`만 검색 결과에 노출한다.

## 4. LEGO Record JSON 저장소

초기 버전은 JSON 파일 저장소를 사용한다.

```text
data/legos/
```

필수 동작:

- 하위 폴더 포함 `*.json` 재귀 로드
- 파일별 오류 격리
- 중복 `LegoId` 검출
- 존재하지 않는 `TemplateId` 검출
- Template에 없는 Property Key 검출
- Property 값을 Template DataType에 맞게 검증
- 잘못된 Record가 정상 Record 검색을 막지 않도록 처리
- 향후 SQLite로 교체할 수 있도록 저장소 책임 분리

등록·편집 UI는 이번 Task에서 만들지 않는다.

## 5. 샘플 LEGO Record

기존 Template와 연결되는 Record를 **최소 8개** 작성한다.

- 파형강관 1개 이상
- 집수정 3개 이상
- 옹벽 4개 이상

서로 다른 검색·필터 결과가 나오도록 값을 구성한다. 샘플 값은 실제 설계 표준이 아니라 기능 시험용임을 문서에 명시한다.

---

# PART B — 검색 엔진

## 6. 검색 요청

최소 요소:

- `Keyword`
- `TemplateId`
- `Category`
- `Filters`
- `Sort`

필터 지원:

| DataType | 조건 |
|---|---|
| Text | 정확히 일치, 포함 |
| Number | 정확히 일치, 최소, 최대, 범위 |
| Boolean | 전체, 예, 아니오 |
| Select | 전체, 하나 이상 선택 |

## 7. 키워드 검색 대상

대소문자를 구분하지 않고 다음을 검색한다.

- `LegoId`
- `Name`
- `Description`
- `Category`
- `Tags`
- 사용자에게 표시 가능한 Property 값
- Template 이름
- Template Field의 `DisplayName`

한글 부분 일치 검색을 지원한다. 형태소 분석, AI 검색, 퍼지 검색, 초성 검색은 제외한다.

## 8. 검색 결합 규칙

```text
Keyword
AND Template
AND Category
AND 각 Field 필터
```

같은 Select 필터에서 여러 값을 선택한 경우만 OR로 처리한다.

## 9. 정렬

최소 지원:

- 관련도순
- 이름 오름차순
- 이름 내림차순
- 최근 수정순

관련도 우선순위:

1. LegoId 정확히 일치
2. Name 정확히 일치
3. Name 시작 일치
4. Name 포함
5. Tags 일치
6. Description, Category, Property 값 일치

결과는 결정론적이어야 한다.

## 10. 검색 결과

최소 표시 정보:

- LegoId
- 이름
- Template 이름
- Category
- 주요 규격 요약
- Tags
- ThumbnailPath
- AssetPath
- UpdatedAt

주요 규격 요약은 Template의 `Searchable = true` Field를 `SortOrder` 순으로 조합한다. 구조물별 문자열을 하드코딩하지 않는다.

---

# PART C — 최종 승인 UI

## 11. UI 기술

```text
C# / .NET 8 / WinForms
```

권장 프로젝트:

```text
src/ProjectLEGO.Desktop
```

- `ProjectLEGO.Core`: 모델, Loader, Validator, Search
- `ProjectLEGO.Desktop`: 화면과 사용자 상호작용

## 12. 화면 배치

```text
┌─────────────────────────────────────────────────────────────┐
│ Project LEGO                                                │
├─────────────────────────────────────────────────────────────┤
│ [검색어 입력................................] [검색] [초기화] │
├───────────────┬─────────────────────────────────────────────┤
│ LEGO 종류     │ 검색 결과                        결과 N개    │
│ ○ 전체        │ [미리보기] 이름 / 규격 / 분류 / 태그        │
│ ○ 파형강관    │                                      [선택] │
│ ○ 집수정      │ ─────────────────────────────────────────── │
│ ○ 옹벽        │ [미리보기] 이름 / 규격 / 분류 / 태그        │
│               │                                      [선택] │
│ 상세 필터     │                                             │
│ (동적 생성)   │                                             │
│               │                                             │
│ 정렬          │                                             │
├───────────────┴─────────────────────────────────────────────┤
│ 선택: LEGO 이름 / LegoId             [폴더 열기] [삽입 요청] │
└─────────────────────────────────────────────────────────────┘
```

정확한 픽셀과 색상은 가독성 범위에서 조정할 수 있으나 화면 흐름과 역할은 변경하지 않는다.

## 13. UI 동작

### 최초 실행

- Template와 LEGO Record를 로드한다.
- 전체 활성 LEGO를 표시한다.
- 오류 파일이 있어도 정상 데이터는 계속 사용한다.
- 오류 건수는 상태 영역에 표시한다.

### 검색

- Enter 또는 검색 버튼으로 실행
- 빈 검색어는 전체 검색
- 입력 즉시 검색은 이번 버전에서 구현하지 않는다.

### LEGO 종류

- 전체 선택 시 상세 필터를 숨기거나 비활성화한다.
- 특정 Template 선택 시 `Searchable = true` Field로 필터 UI를 생성한다.
- `SortOrder`를 따른다.

### DataType별 컨트롤

| DataType | UI |
|---|---|
| Text | TextBox + 비교 방식 |
| Number | 최소·최대 Numeric 입력 |
| Boolean | 전체/예/아니오 ComboBox |
| Select | 전체 또는 다중 선택 목록 |

Unit이 있으면 필드명 옆에 표시한다.

### 결과

각 결과에 다음을 표시한다.

- 썸네일 또는 Placeholder
- LEGO 이름
- Template 이름
- Category
- 주요 규격 요약
- Tags
- 선택 기능

0건이면 다음 메시지를 표시한다.

```text
조건에 맞는 LEGO가 없습니다.
검색어나 상세 필터를 변경해 보세요.
```

### 선택

- 선택 상태를 강조한다.
- 하단에 이름과 LegoId를 표시한다.
- 더블클릭은 삽입 요청과 동일하게 처리한다.
- 미선택 시 삽입 요청 버튼은 비활성화한다.

### 폴더 열기

- `AssetPath`의 파일 또는 상위 폴더를 연다.
- 경로가 없거나 잘못되어도 앱이 종료되지 않아야 한다.
- 샘플 경로가 존재하지 않을 수 있으므로 오류 처리를 반드시 구현한다.

### 삽입 요청

실제 CAD 삽입은 하지 않는다.

다음 역할의 이벤트 또는 인터페이스를 구현한다.

```csharp
LegoInsertRequested
```

최소 포함 정보:

- `LegoId`
- `TemplateId`
- `AssetPath`
- `Properties`

이벤트 발생 후 UI 메시지:

```text
삽입 요청이 생성되었습니다.
실제 ZWCAD 삽입 기능은 후속 단계에서 연결됩니다.
```

향후 ZWCAD Adapter가 UI 코드를 수정하지 않고 요청을 받을 수 있어야 한다.

### 초기화

검색어, Template, 상세 필터, 정렬, 선택 결과를 모두 초기 상태로 되돌린다.

---

# PART D — 권장 구조

## 14. 권장 산출물

```text
src/
├─ ProjectLEGO.Core/
│  ├─ Templates/
│  └─ Legos/
│     ├─ LegoRecord.cs
│     ├─ LegoRecordLoader.cs
│     ├─ LegoRecordValidator.cs
│     ├─ LegoRecordLoadResult.cs
│     ├─ ILegoRepository.cs
│     ├─ JsonLegoRepository.cs
│     └─ Search/
│        ├─ LegoSearchService.cs
│        ├─ LegoSearchRequest.cs
│        ├─ LegoSearchFilter.cs
│        ├─ LegoSearchResult.cs
│        └─ LegoSortOption.cs
│
└─ ProjectLEGO.Desktop/
   ├─ Program.cs
   ├─ MainForm.cs
   ├─ MainForm.Designer.cs
   ├─ Controls/
   │  ├─ DynamicFilterPanel.cs
   │  └─ LegoResultCard.cs
   └─ Services/
      └─ DesktopInsertRequestDispatcher.cs

data/
├─ templates/
└─ legos/

tests/
└─ ProjectLEGO.Core.Tests/
   └─ Legos/
```

기존 저장소 구조와 충돌하면 기존 네이밍과 구조를 우선한다.

### WinForms 분리 원칙

- `MainForm.cs`: 이벤트 처리와 화면 로직
- `MainForm.Designer.cs`: 컨트롤 선언과 배치
- 데이터 로드·검색: 별도 서비스
- 동적 필터·결과 카드: 별도 사용자 컨트롤

모든 코드를 `MainForm.cs` 하나에 몰아넣지 않는다.

---

# PART E — 테스트

## 15. 필수 자동 테스트

기존 xUnit 체계를 사용한다.

### Loader / Validator

1. 정상 Record 로드
2. 하위 폴더 로드
3. 오류 파일 격리
4. 중복 LegoId 검출
5. 없는 TemplateId 검출
6. 없는 Property 검출
7. Number·Boolean·Select 값 검증
8. 비활성 Record 검색 제외
9. 한글 이름·Tags 보존

### Search

10. 빈 검색어 전체 검색
11. 이름 부분 일치
12. Tag 검색
13. 한글 검색
14. Template 필터
15. Category 필터
16. Number 범위
17. Boolean 필터
18. Select 다중 선택 OR
19. 서로 다른 필터 AND
20. 관련도 정렬의 결정성
21. 이름·최근 수정 정렬
22. 규격 요약 SortOrder
23. 새 Template와 Record가 구조물별 코드 수정 없이 검색됨

### Insert Request

24. 선택 LEGO에서 요청 모델 생성
25. 미선택 상태에서 요청 발생 안 함

## 16. UI 수동 시험

다음을 확인하고 문서화한다.

- 앱 실행
- 검색 결과 표시
- Enter 검색
- Template 선택에 따른 필터 변경
- Number/Boolean/Select 필터
- 0건 메시지
- 선택 강조
- 삽입 요청 버튼 활성화·비활성화
- 더블클릭 삽입 요청
- 초기화
- 잘못된 AssetPath 처리
- 고해상도 배율에서 컨트롤 잘림 여부
- 창 크기 변경 시 레이아웃 유지

문서:

```text
docs/TEST/TASK-0003_LEGO_SEARCH_UI_TEST.md
```

---

# PART F — 완료 기준

## 17. 완료 조건

- [ ] TASK-0002 Template Engine 재사용
- [ ] LegoRecord와 JSON Loader 구현
- [ ] Record 검증과 오류 격리
- [ ] 샘플 Record 8개 이상
- [ ] UI와 분리된 Search Service
- [ ] Keyword·Template·Category·상세 필터
- [ ] Template Searchable Field 기반 동적 필터
- [ ] 관련도 및 지정 정렬
- [ ] WinForms Desktop 프로젝트 추가
- [ ] 승인된 검색 화면 구현
- [ ] 결과 선택과 빈 상태
- [ ] 폴더 열기 오류 처리
- [ ] Insert Request 이벤트 또는 인터페이스
- [ ] 실제 CAD 삽입 제외
- [ ] 자동 테스트 통과
- [ ] Release Build 성공
- [ ] UI 수동 시험 문서
- [ ] README와 CHANGELOG 업데이트
- [ ] Commit·Push
- [ ] 가능하면 작업 브랜치와 Pull Request 사용
- [ ] 최종 작업 트리 깨끗함

---

# PART G — 금지 사항

## 18. 구현 금지

- ZWCAD/AutoCAD API 연동
- 실제 DWG 삽입
- LEGO 등록·수정·삭제 UI
- Template 편집 UI
- SQLite 또는 서버 DB
- NAS 자동 검색·동기화
- DWG 내용 분석
- 썸네일 자동 생성
- OCR
- AI·벡터·초성 검색
- 사용자 계정·권한
- 클라우드 동기화
- 구조물별 하드코딩 검색 UI
- 저장소 대규모 재편

새 아이디어는 이번 Task에 넣지 말고 후속 Task 제안으로만 기록한다.

---

# PART H — 작업 절차와 완료 보고

## 19. 작업 절차

1. 원격 main과 로컬 상태 확인
2. TASK-0002 실제 코드 분석
3. 가능하면 `feature/task-0003-lego-search` 브랜치 생성
4. Core의 Record와 Search 구현
5. 자동 테스트 작성 및 통과
6. Desktop WinForms와 UI 구현
7. 샘플 데이터로 실제 실행
8. UI 수동 시험 및 문서화
9. README·CHANGELOG 수정
10. Restore·Release Build·Test 실행
11. `git diff --check`와 작업 트리 확인
12. Commit·Push 및 가능하면 PR 생성
13. 완료 보고 제출

## 20. 중단 기준

아래 상황에서는 임의로 우회하지 말고 보고한다.

- TASK-0002 API를 재사용할 수 없는 경우
- Target Framework에서 WinForms 구성이 불가능한 경우
- 기존 Solution 또는 CI를 깨뜨려야 하는 경우
- 공개 저장소에 고객 도면이나 기밀 데이터를 넣어야 하는 경우
- AssetPath 정책에 Y대표 결정이 필요한 경우

샘플 가상 경로와 Placeholder로 진행 가능하면 중단하지 않는다.

## 21. 완료 보고 형식

```md
# TASK-0003 완료 보고

## 1. 결과
- 완료 / 부분 완료 / 차단

## 2. Branch / Pull Request
- Branch
- PR
- Merge 여부

## 3. 변경 파일
- 경로와 목적

## 4. 구현 내용
- LEGO Record
- JSON Repository
- Validator
- Search Engine
- Dynamic Filter
- WinForms UI
- Insert Request

## 5. 실제 화면
- 실행 방법
- 화면 캡처 위치
- 검색·필터 예시

## 6. 검증 결과
- Restore
- Release Build
- Test
- UI 수동 시험
- 경고·오류

## 7. 샘플 데이터
- Template별 Record 수
- 검색 예시
- AssetPath 처리

## 8. 설계 판단
- 기존 구조에 맞춰 조정한 내용과 이유

## 9. 범위 준수
- 제외 항목
- CAD 연동 미포함 확인

## 10. Git 결과
- Commit 메시지
- SHA
- Push
- 작업 트리

## 11. 남은 사항
- 제한
- 후속 Task 제안
```

---

## 22. 최종 검증 시나리오

> 사용자가 Project LEGO Desktop을 실행하고 `집수정`을 검색한 뒤 집수정 Template를 선택한다. Template의 Searchable 필드인 가로·세로·높이·재질·형태 필터가 자동 생성된다. 규격 조건을 입력하면 조건에 맞는 등록 LEGO만 표시된다. 하나를 선택해 `삽입 요청`을 누르면 LegoId·TemplateId·AssetPath·Properties를 포함한 요청 이벤트가 발생한다. 이 과정에서 구조물별 검색 UI를 하드코딩하지 않는다.

이 시나리오가 실제 실행되어야 TASK-0003을 완료로 인정한다.
