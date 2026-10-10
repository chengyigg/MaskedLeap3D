《MaskedLeap3D》技术设计文档



Version 1.0 · 2026-10-10 · 石诚意



项目信息

项	内容

项目名称	MaskedLeap3D

开发引擎	Unity 2021 LTS+（Built-in 渲染管线）

开发语言	C#

项目类型	3D 第三人称平台跳跃游戏原型

开发周期	约 2 周（个人独立开发）

作者	石诚意

仓库	https://github.com/chengyigg/MaskedLeap3D

一、项目概述

1.1 项目简介

《MaskedLeap3D》是一款 3D 第三人称平台跳跃游戏原型，玩家操控一名 Q 版小丑角色，在关卡中收集金币、躲避敌人与陷阱、利用弹簧与移动平台前进，最终抵达终点。



项目目标：从 2D 项目迁移到 3D，验证 3D 游戏开发的核心能力——角色控制器、摄像机系统、3D 物理交互、对象池、事件驱动、ScriptableObject 配置。



1.2 技术目标

专业级角色手感：实现平台跳跃游戏的标准手感方案（土狼时间、跳跃缓冲、可变跳跃高度等）



数据驱动的参数配置：把玩家参数抽离为 ScriptableObject 资源，不写死在代码里



对象池与事件驱动：3D 项目中复用 2D 项目的架构经验



1.3 技术栈

层	技术

引擎	Unity（Built-in 渲染管线）

语言	C#

UI	TextMeshPro

角色控制	Unity Character Controller

粒子	ParticleSystem + 程序化生成贴图

数据配置	ScriptableObject

事件系统	C# event + 静态事件中心

设计模式	对象池 / 状态机 / 观察者 / 单例

版本控制	Git / GitHub

二、系统架构

2.1 整体架构

text

┌─────────────────────────────────────┐

│         PlayerController            │

│  （角色控制核心：移动 / 跳跃 / 冲刺）  │

└──────────┬──────────────────────────┘

&#x20;          │

&#x20;          ├──→ PlayerConfig (ScriptableObject)  读参数

&#x20;          │

&#x20;          ├──→ GameEvents                       触发事件

&#x20;          │        │

&#x20;          │        ├──→ CoinCounterUI          订阅 OnCoinCollected

&#x20;          │        ├──→ DeathFade              订阅 OnPlayerDied

&#x20;          │        ├──→ GameManager            订阅 OnPlayerWin

&#x20;          │        └──→ CameraController       订阅 OnPlayerBounced

&#x20;          │

&#x20;          ├──→ CameraController                 摄像机跟随

&#x20;          │

&#x20;          └──→ 游戏对象交互

&#x20;                   ├──→ Coin           （对象池管理）

&#x20;                   ├──→ Enemy / ChaseEnemy （对象池管理）

&#x20;                   ├──→ Spring

&#x20;                   ├──→ MovingPlatform

&#x20;                   ├──→ Trap

&#x20;                   └──→ Checkpoint

架构核心：



PlayerController 只管移动，不关心金币、敌人、UI



系统间通过 GameEvents 通信，不互相持有引用



参数抽到 ScriptableObject，策划改数值不改代码



频繁创建的对象用池化，减少 GC 压力



2.2 分层结构

text

Assets/

├── Scenes/

│   └── SampleScene.unity

├── Scripts/

│   ├── Player/          角色控制 + 冲刺残影

│   ├── Camera/          第三人称摄像机

│   ├── Environment/     金币、弹簧、移动平台、陷阱、存档点、终点

│   ├── Hazards/         巡逻敌人、追击敌人（状态机）

│   ├── Managers/        GameManager / AudioManager / PauseMenu

│   ├── UI/              金币计数、死亡淡出、按钮音效、教学提示

│   ├── Config/          PlayerConfig（ScriptableObject）

│   └── Utils/           对象池、程序化粒子、事件中心

├── Prefabs/

├── Art/

├── Audio/

├── Fonts/

└── Editor/              自定义 Editor 工具

三、核心模块设计

3.1 角色控制器（PlayerController）

3.1.1 参数配置（ScriptableObject）

PlayerConfig 资源文件包含以下字段：



字段	含义	默认值

moveSpeed	移动速度	—

airControlMultiplier	空中控制衰减系数	0.6

jumpHeight	一段跳高度	2 m

doubleJumpHeight	二段跳高度	1.5 m

gravity	重力加速度	—

maxJumpCount	最大跳跃次数	2

coyoteTime	土狼时间	0.15 s

jumpBufferTime	跳跃缓冲	0.1 s

variableJumpMultiplier	可变跳跃高度	0.5

dashSpeed	冲刺速度	18

dashDuration	冲刺时长	0.15 s

dashCooldown	冲刺冷却	1 s

为什么用 ScriptableObject：策划改数值不用动代码，不用重新编译；一份配置可被多个实例共用；在 Inspector 里实时调参，一边玩一边改。



3.1.2 专业级跳跃手感

这是 3D 平台跳跃游戏的核心。参考行业成熟方案（如《超级马里奥 奥德赛》《蔚蓝》），实现了以下机制：



机制	作用	实现方式

土狼时间	离开平台边缘后 0.15s 内仍可跳	用计时器记录"最后一次在地面的时间"，离地后计时未超时即可跳

跳跃缓冲	落地前 0.1s 按跳，落地瞬间自动触发	记录"最后一次按跳时间"，落地时检测是否在缓冲窗口内

可变跳跃高度	松手时垂直速度削减，按住跳更高	上升过程中检测按键是否松开，松开时 velocity.y 乘以 variableJumpMultiplier

空中控制衰减	空中转向有惯性感	空中输入乘以 airControlMultiplier

落地顿挫	落地瞬间模型 Scale 压扁再弹回	用协程在 0.1s 内把 Scale 从 (1.2, 0.8, 1.2) 恢复到 (1, 1, 1)

移动平台跟随	玩家踩上去被带着走，无滑落	射线检测脚下的平台，把平台速度合并到玩家速度

二段跳	比一段跳矮，符合直觉	二段跳时使用 doubleJumpHeight 而非 jumpHeight

3.1.3 冲刺机制

触发：左 Shift



方向：摄像机正前方（不是角色朝向）



表现：FOV 从 60° 平滑扩到 75°（速度感）；身后拖蓝色半透明残影（GhostTrail 生成，GhostFade 淡出）



冷却：1 秒



残影实现：每隔 0.05 秒在当前位置生成一个角色模型的"快照"（复制 Mesh + 半透明材质），0.3 秒后淡出销毁。



3.2 第三人称摄像机（CameraController）

3.2.1 层级结构

text

CameraPivot（独立根物体，跟随玩家位置）

└── Main Camera（负责旋转和缩放）

3.2.2 关键设计：为什么 Pivot 要独立

第一版实现：把 Camera 作为玩家的子物体。问题：玩家旋转时摄像机跟着转，鼠标再控制摄像机旋转，两个旋转叠加导致视角乱飞。



解决方案：CameraPivot 是独立根物体，在 LateUpdate 里读取玩家位置并跟随（不改玩家层级）。这样：



摄像机跟随玩家位置 ✅



摄像机旋转独立控制 ✅



玩家转向不影响摄像机 ✅



3.2.3 功能

操作	效果

鼠标左右滑	Pivot 绕 Y 轴旋转（水平视角）

鼠标上下滑	Main Camera 绕 X 轴旋转（俯仰限制 -30° \~ 70°）

滚轮	Main Camera Z 轴缩放（限制 -2 \~ -10）

冲刺时	FOV 平滑扩张

落地时	摄像机微震

3.3 对象池系统（ObjectPool）

3.3.1 设计

text

PooledObject（基类）

&#x20; ├── OnSpawn()          被取出时调用

&#x20; ├── OnDespawn()        归还时调用

&#x20; └── ReturnToPool()     归还到池



ObjectPool（管理器）

&#x20; ├── PrewarmCount       预创建数量

&#x20; ├── Get()              取出

&#x20; ├── Return()           归还

&#x20; └── 池空时自动创建

3.3.2 为什么用对象池

关卡中金币和敌人会频繁创建和销毁：玩家吃金币、敌人被踩死、死亡重生时金币重新出现。



每帧 Instantiate / Destroy 会造成：内存碎片、GC 压力（Unity 的 GC 是 Stop-the-World，卡顿）、实例化开销。



对象池方案：预创建一批物体，用时取出、不用时隐藏归还。关卡内零 Instantiate。



3.4 事件驱动系统（GameEvents）

3.4.1 设计

静态事件中心，用 C# event 解耦系统通信。



text

GameEvents（静态类）

&#x20; ├── OnCoinCollected    金币被吃

&#x20; ├── OnPlayerDied       玩家死亡

&#x20; ├── OnPlayerWin        玩家通关

&#x20; └── OnPlayerBounced    玩家被弹跳

3.4.2 使用规范

订阅在 OnEnable，取消订阅在 OnDisable。为什么：避免对象销毁后事件还持有引用（内存泄漏）。



3.4.3 解耦效果

以"玩家吃金币"为例：



text

Coin.OnTriggerEnter()

&#x20; └─ GameEvents.CoinCollected()

&#x20;       ├── CoinCounterUI 订阅 → 更新数字

&#x20;       └── AudioManager 订阅 → 播放音效

金币完全不知道 UI 和音频系统的存在。以后加"金币收集成就系统"只需多订阅一个事件，金币代码一行不改。



3.5 敌人 AI 状态机（ChaseEnemy）

3.5.1 三状态

text

Patrol ──玩家进入检测范围──→ Chase

&#x20;  ↑                            │

&#x20;  │                            │

&#x20;  └──────玩家超出范围──────────┘

&#x20;             Return

状态	行为

Patrol	在 A、B 两点之间用 Mathf.PingPong(patrolT, 1f) 巡逻

Chase	检测到玩家（距离 < detectRange）切换追击

Return	玩家超出范围（距离 > detectRange \* 1.2）平滑走回巡逻路线，不瞬移

3.5.2 关键设计：巡逻进度手动维护

问题：如果巡逻用 Mathf.PingPong(Time.time, 1f)，追击时时间还在走，返回时敌人会瞬移到"时间对应的位置"。



解决方案：用变量 patrolT 手动累加，追击时冻结，返回时按冻结进度算目标位置，平滑走回去。



3.5.3 踩头判定

text

if (玩家正在下落 \&\& 玩家高度 > 敌人高度 + 阈值)

&#x20;   踩死敌人 + 玩家弹起

else

&#x20;   玩家死亡

3.6 通用陷阱（Trap）

3.6.1 三种模式可自由组合

参数	效果

isRotating	绕轴旋转

isMoving	A、B 两点之间移动

isToggling	显隐交替

3.6.2 扩展参数

startDelay：初始延迟（错开多个陷阱的节奏，避免全同步）



startVisible：初始是否可见



3.6.3 去重设计

问题：一个陷阱可能有多个检测方块（比如旋转锯片有多段），玩家同时碰到多个会触发多次死亡。



解决：用 HashSet 记录已触发玩家，同一玩家只触发一次。



3.7 程序化粒子（ParticleBurst）

问题：常规做法需要美术提供圆形渐变贴图，但这是个人项目，没有美术资源。



解决方案：运行时用代码生成圆形渐变贴图。



原理：用 Texture2D.SetPixels() 生成一个像素级 alpha 渐变的圆，Sprite.Create() 成 Sprite，赋给粒子系统的材质。



优势：零美术资源依赖；任意颜色、任意大小；代码量小（一个静态工具类）。



四、关键技术难点

4.1 移动平台无滑落方案

问题

Unity CharacterController 自带摩擦力处理，但在移动平台上会滑落——玩家踩在平台上，平台移动时玩家不动，慢慢滑下来。



解决方案

不用物理摩擦，用射线检测 + 速度合并：



每帧从玩家脚下打一条向下的射线



检测是否命中平台



如果命中：获取平台速度



把平台速度合并到玩家移动速度中



玩家跟随平台移动



为什么这样更好：不依赖物理引擎的摩擦参数（调起来很烦）；移动精确（平台多快，玩家就多快）；无滑落、无抖动。



4.2 摄像机旋转死循环

问题

第一版把 Camera 作为玩家的子物体。玩家转向时摄像机跟着转，鼠标控制摄像机旋转时又叠加了一次旋转。结果视角乱飞。



解决方案

CameraPivot 独立为根物体，LateUpdate 里读取玩家位置：



text

void LateUpdate()

{

&#x20;   transform.position = player.position + offset;

}

关键点：LateUpdate 而不是 Update（保证在玩家移动之后跟随）；不改变玩家层级（摄像机不是玩家的子物体）。



4.3 跳跃手感的调试

问题：纯数值调试（一个个改参数试）效率极低。



解决方案：把参数全部抽到 PlayerConfig ScriptableObject，Play 模式下实时调参：



改 coyoteTime 从 0.15 到 0.2，立刻感受变化



改 variableJumpMultiplier 从 0.5 到 0.6，立刻感受



找到手感最好的数值后保存



这比"改代码 → 重新编译 → Play"快 10 倍。



五、测试与验证

5.1 功能测试

功能	测试用例

移动	前后左右移动、空中移动、移动平台跟随

跳跃	一段跳、二段跳、土狼时间、跳跃缓冲、可变高度

冲刺	触发、方向、冷却、FOV 扩张、残影

金币	拾取、计数 UI、磁吸

弹簧	触发弹飞、压缩动画

陷阱	旋转、移动、显隐、去重触发

敌人	巡逻、追击、返回、踩头、侧碰

存档点	触碰后重生位置更新

死亡重生	触发 → 淡黑 → 传送 → 淡回

通关	终点触发 → 通关面板 → 显示用时

暂停	Esc → 菜单 → 继续 / 重开 / 退出

5.2 边界情况

玩家卡在墙里：CharacterController 会自动把玩家推出



同时踩多个弹簧：只会弹一次（协程锁）



边跳边踩敌人：判定为踩死，玩家弹起



死亡瞬间碰到终点：优先判定死亡



5.3 打包验证

打包成 Windows exe（约 250 MB）



在没有 Unity 环境的机器上运行



测试：进入游戏 → 教学提示 → 收集金币 → 跳跃 → 冲刺 → 踩敌人 → 通关



六、总结

6.1 项目收获

2D → 3D 技术迁移：把 2D 项目里的状态机、事件驱动、对象池等模式成功应用到 3D 场景



专业级手感设计：学会用土狼时间、跳跃缓冲、可变跳跃高度等成熟方案



3D 摄像机系统：理解了 Pivot 独立层级的重要性



ScriptableObject 数据驱动：把参数从代码中彻底抽离



程序化生成资源：没有美术资源也能做出好看的粒子效果



6.2 可优化方向

角色骨骼动画：目前模型是静态的，加动画会更生动



多关卡系统：目前只有单关卡



Post-Processing：加 Bloom + Vignette 提升画面质感



数据驱动关卡设计：用 ScriptableObject 定义关卡布局



排行榜 / 计时：记录最佳通关时间

