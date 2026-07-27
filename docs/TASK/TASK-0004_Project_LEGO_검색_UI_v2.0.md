# TASK-0004 — Project LEGO 검색 시스템(UI v2.0) 구현

- **Status**: Approved / Implementation
- **Assignee**: C기사(Codex)
- **Approver**: Y대표
- **Author**: G팀장
- **Base**: TASK-0003 LEGO 검색 기능 및 기본 UI

## 목적

기존 계층형 규격 호출 방식을 유지하면서 모든 LEGO를 이름·규격·형식·키워드
일부로 통합 검색할 수 있는 UI v2.0을 구현한다.

## 승인 흐름

```text
통합 검색 → 검색 결과 → 계층 규격 선택 → Preview → 열기 / 삽입
```

## 핵심 요구사항

- WinForms 기본 컨트롤만 사용한다.
- 결과에는 LEGO 이름과 규격1~4를 표시한다.
- 결과 선택 후 Template의 검색 가능 필드를 규격1~4로 동적 구성한다.
- 상위 규격 변경 시 하위 규격을 초기화한다.
- 최종 LEGO가 유일하게 결정된 경우에만 Preview와 열기·삽입을 활성화한다.
- 검색은 이름, 규격, 설명, 태그에 대해 대소문자·공백을 무시한 부분 일치를 지원한다.
- `×`, `x`, `X` 표기를 정규화한다.
- UI는 JSON을 직접 읽지 않고 `SearchService → Repository` 계층을 사용한다.
- 실제 ZWCAD 삽입, Thumbnail, NAS, SQLite는 포함하지 않는다.

## 완료 조건

- 통합 검색, 계층 규격 선택, Preview, Mock 열기·삽입 구현
- Repository 구조 및 SearchService 분리
- Release Build와 전체 Test 성공
- Commit·Push 및 깨끗한 작업 트리
