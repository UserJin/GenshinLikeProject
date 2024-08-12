# GenshinLikeProject
## 1. 소개
 * "__HoYoverse__"에서 개발한 게임 "__원신__"에서 사용되는 캐릭터의 조작법과 기본 이동 방식(걷기, 달리기, 정지, 질주, 점프)을 구현하는 것을 목적으로 하는 프로젝트 입니다.
 * Youtube의 "**Genshin Impact Movement - Indie Wafflus**" 영상 시리즈를 보고 따라 구현하는 방식으로 진행하였습니다.
 * 개발 기간: 2024-07-15 ~ 2024-08-10<br/><br/>

## 2. 개발 환경
 * Unity 2022.3.7f1 LTS
 * C#
 * Windows 10<br/><br/>

## 3. 구현 기능
 * 조작 및 입력
 * 상태
 * 이동
 * 카메라
 * 애니메이션
   * Mixamo
 

## 4. 프로젝트 후기
 * 본 영상 시리즈에서는 기본 이동 방식 중, 활공 및 수영에 대한 구현에 대해서는 다루지 않았다. 하지만 해당 상태들 또한 본 영상에서 구현한 상태들과 큰 차이는 없기에 유사한 방식으로 구현하는 것이 가능하다고 생각된다.
   Ex) 활공과 수영 상태로 전환되는 조건을 지정, 활공 상태(캐릭터에 가해지는 중력 값 조정, 조작 방식을 입력한 방향으로 선회하는 방식으로 수정), 수영 상태()
 * 지금까지 여러 강의 및 책을 통해 공부하면서 State Machine을 몇 번 구현한 경험이 있으나, 대부분 Enum형식으로 state를 선언하고 switch나 if문을 통해 각 state의 분기에 따른 기능을 실행하는 방식으로 구현하였다.
   이번에 구현한 프로젝트에서는 Interface를 적극적으로 활용하여 

 참고 영상 : **Genshin Impact Movement - Indie Wafflus** (https://www.youtube.com/playlist?list=PL0yxB6cCkoWKuPoh_9dSvdItQENVx7YTW)
