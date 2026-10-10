 MaskedLeap3D

> 一个 Unity 3D 第三人称平台跳跃游戏原型 —— 展示角色控制器、摄像机系统、对象池、事件驱动等 3D 游戏开发核心能力。

🎬 **[观看完整演示视频（B 站）](https://www.bilibili.com/video/BV1xTpG6iEDa)**


\---



\## 项目简介



\- \*\*引擎\*\*：Unity 2021 LTS+（Built-in 渲染管线）

\- \*\*语言\*\*：C#

\- \*\*类型\*\*：3D 第三人称平台跳跃游戏原型

\- \*\*目标\*\*：从 2D 迁移到 3D，验证 3D 游戏开发核心能力

\- \*\*开发者背景\*\*：计算机专业大三学生，此前完成过两个 2D 项目（Godot 卡牌 RPG、Unity 文字冒险 AVG）



\---



\## 技术栈



| 层 | 技术 |

|---|---|

| 引擎 | Unity（Built-in 渲染管线） |

| 语言 | C# |

| UI | TextMeshPro |

| 角色控制 | Unity Character Controller |

| 粒子 | ParticleSystem + 程序化生成贴图 |

| 数据配置 | ScriptableObject |

| 事件系统 | C# event + 静态事件中心 |

| 设计模式 | 对象池、状态机、观察者、单例 |



\---



\## 核心系统



\### 1. 角色控制器（`PlayerController.cs`）



基于 `CharacterController` 的第三人称角色控制，参数通过 `PlayerConfig` ScriptableObject 配置。



\*\*专业级跳跃手感\*\*：

\- \*\*土狼时间（Coyote Time）\*\*：离开平台边缘后 0.15 秒内仍可跳跃

\- \*\*跳跃缓冲（Jump Buffer）\*\*：落地前 0.1 秒按跳，落地瞬间自动触发

\- \*\*可变跳跃高度\*\*：松手时垂直速度削减，按住跳更高

\- \*\*空中控制衰减\*\*：空中转向有惯性感

\- \*\*落地顿挫\*\*：落地瞬间模型 Scale 压扁再弹回

\- \*\*移动平台跟随\*\*：射线检测平台，速度合并，无滑落

\- \*\*二段跳\*\*：比一段跳矮，符合直觉



\*\*冲刺机制\*\*：左 Shift 触发，方向为摄像机正前方；冲刺时 FOV 从 60 平滑扩到 75，身后拖蓝色半透明残影（`GhostTrail` + `GhostFade`）。



\### 2. 第三人称摄像机（`CameraController.cs`）



\*\*层级结构\*\*：



```

CameraPivot（独立根物体，跟随玩家位置）

└── Main Camera（负责旋转和缩放）

```



\*\*关键设计\*\*：`CameraPivot` 是独立根物体，通过 `LateUpdate` 跟随玩家位置，\*\*避免父子关系导致的旋转死循环\*\*。



\*\*功能\*\*：

\- 鼠标左右滑：Pivot 绕 Y 轴旋转（水平视角）

\- 鼠标上下滑：Main Camera 绕 X 轴旋转（俯仰限制 -30° \~ 70°）

\- 滚轮：Main Camera Z 轴缩放（限制 -2 \~ -10）

\- 冲刺时 FOV 平滑扩张

\- 落地时摄像机微震



\### 3. 对象池系统（`ObjectPool.cs` + `PooledObject.cs`）



\- \*\*`PooledObject` 基类\*\*：`OnSpawn()` / `OnDespawn()` / `ReturnToPool()`

\- \*\*`ObjectPool` 管理器\*\*：预创建 `PrewarmCount` 个物体，`Get()` 取出，`Return()` 归还，池空时自动创建

\- \*\*已池化对象\*\*：金币、巡逻敌人、追击敌人



\*\*解决的问题\*\*：频繁 Instantiate / Destroy 会造成 GC 压力，池化后减少内存碎片。



\### 4. 事件驱动系统（`GameEvents.cs`）



静态事件中心，用 `event Action` 解耦系统通信。



| 事件 | 触发时机 |

|---|---|

| `OnCoinCollected` | 金币被吃 |

| `OnPlayerDied` | 玩家死亡 |

| `OnPlayerWin` | 玩家通关 |

| `OnPlayerBounced` | 玩家被弹跳 |



\*\*使用规范\*\*：订阅在 `OnEnable`，取消订阅在 `OnDisable`，避免内存泄漏。



\### 5. ScriptableObject 配置（`PlayerConfig.cs`）



把玩家参数从代码和场景中抽离为资源文件。



\*\*优势\*\*：

\- 策划改数值不用动代码

\- 一份配置可被多个实例共用

\- 可在 Inspector 里实时调参



\### 6. 敌人 AI 状态机（`ChaseEnemy.cs`）



三状态：\*\*Patrol → Chase → Return\*\*



\- \*\*Patrol\*\*：在 A、B 两点之间用 `Mathf.PingPong` 巡逻

\- \*\*Chase\*\*：检测到玩家（距离 < `detectRange`）切换追击

\- \*\*Return\*\*：玩家超出范围（距离 > `detectRange \* 1.2`）平滑走回巡逻路线，\*\*不瞬移\*\*



\*\*关键设计\*\*：巡逻进度用 `patrolT` 手动维护，追击时冻结，返回时按冻结进度算目标位置。



\*\*踩头判定\*\*：玩家下落 + 高度高于敌人 → 踩死；否则 → 玩家死亡。



\### 7. 通用陷阱（`Trap.cs`）



三种模式可自由组合：

\- `isRotating`：绕轴旋转

\- `isMoving`：A、B 两点之间移动

\- `isToggling`：显隐交替



\*\*扩展参数\*\*：`startDelay`（错开多个陷阱的节奏）、`startVisible`。



\*\*去重设计\*\*：多个检测方块时用 `HashSet` 记录已触发玩家，避免重复触发。



\### 8. 音频系统（`AudioManager.cs`）



单例管理器，两个 `AudioSource`（SFX / BGM）。



\*\*音效\*\*（中文命名）：跳跃、二段跳、落地、冲刺、金币、弹簧、踩敌人、受伤、存档点、重生、通关、按钮点击、按钮悬停、暂停开关、胜利号角。



\### 9. UI 系统



| 脚本 | 功能 |

|---|---|

| `CoinCounterUI` | 金币计数（吃到时数字跳动） |

| `DeathFade` | 死亡淡黑 → 传送 → 淡回 |

| `TutorialHint` | 开局 4 秒教学提示后淡出 |

| `ButtonSFX` | 按钮悬停 / 点击音效 |

| `PauseMenu` | Esc 暂停，含继续 / 重开 / 退出 |



\### 10. 程序化粒子（`ParticleBurst.cs`）



静态工具类，运行时生成圆形渐变贴图和材质，避免依赖美术资源。



调用：`ParticleBurst.Play(position, color, count, speed)`



\---



\## 玩法



| 系统 | 说明 |

|---|---|

| 移动 | WASD 相对摄像机方向平移，角色朝向跟随摄像机 |

| 跳跃 | 空格，一段跳 + 二段跳 + 土狼时间 + 跳跃缓冲 |

| 冲刺 | 左 Shift，高速位移，带残影 + FOV 扩张 |

| 金币收集 | 自动磁吸 + 旋转浮动 + 数字跳动 UI |

| 弹簧 | 踩到弹飞，带压缩动画 |

| 移动平台 | 玩家踩上去被带着走，无滑落 |

| 陷阱 | 旋转 / 移动 / 显隐，触碰即重生 |

| 敌人 | 巡逻 / 追击，踩头杀死，侧面触碰玩家死亡 |

| 存档点 | 触碰后更新重生位置 |

| 终点 | 触发通关面板，显示用时 |

| 死亡重生 | 淡黑 → 传送 → 淡回 |

| 暂停菜单 | Esc 弹出，含继续 / 重开 / 退出 |



\*\*操作方式\*\*：



| 操作 | 按键 |

|---|---|

| 移动 | WASD |

| 跳跃 | 空格（可二段跳） |

| 冲刺 | 左 Shift |

| 视角旋转 | 鼠标 |

| 拉近拉远 | 滚轮 |

| 暂停 | Esc |



\---



\## 技术亮点



1\. \*\*3D 第三人称摄像机系统\*\*：独立 Pivot 层级 + 鼠标旋转 + 滚轮缩放 + FOV 动态扩张，避免父子关系死循环

2\. \*\*专业级跳跃手感\*\*：土狼时间、跳跃缓冲、可变跳跃高度、空中控制衰减、落地顿挫 —— 3D 平台跳跃游戏的标准手感方案

3\. \*\*移动平台无滑落方案\*\*：射线检测平台 + 速度合并，绕过 `CharacterController` 的摩擦力限制

4\. \*\*对象池模式\*\*：频繁创建/销毁的金币、敌人用池化管理，减少 GC 压力

5\. \*\*事件驱动解耦\*\*：用静态 `GameEvents` 解耦系统通信，2D 项目里的观察者模式在 3D 里同样适用

6\. \*\*ScriptableObject 配置\*\*：把玩家参数抽离为资源文件，数据驱动设计

7\. \*\*敌人 AI 状态机\*\*：Patrol / Chase / Return 三状态，平滑过渡不瞬移

8\. \*\*程序化粒子\*\*：运行时生成圆形贴图和材质，避免依赖美术资源

9\. \*\*场景层级工程化\*\*：`\_Environment` / `\_Interactables` / `\_Hazards` / `\_Player` / `\_Managers` 分类组织

10\. \*\*自定义 Editor 工具\*\*：`SceneInspectorExporter.cs` 用于导出场景信息，辅助调试



\---



\## 项目结构



```

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

├── Prefabs/             预制体

├── Art/                 模型 / 材质 / 贴图

├── Audio/               SFX / BGM

├── Fonts/               TMP 中文字体

└── Editor/              自定义 Editor 工具

```



\---



\## 素材来源



| 部分 | 来源 |

|---|---|

| \*\*代码 / 系统设计\*\* | 本人独立完成 |

| \*\*角色模型\*\* | 腾讯混元 AI 生成（Q 版小丑，GLB 格式） |

| \*\*音效 / BGM\*\* | Suno / ElevenLabs 生成 |

| \*\*字体\*\* | 微软雅黑 TTF 转 TMP 字体资源 |



\---



\## 如何运行



1\. 克隆仓库：

&#x20;  ```bash

&#x20;  git clone https://github.com/chengyigg/MaskedLeap3D.git

&#x20;  ```

2\. 用 Unity Hub 以 \*\*Unity 2021 LTS 或更高\*\*打开项目

3\. 打开 `Assets/Scenes/SampleScene.unity`

4\. 点击 Play



\---



\## 已知限制与未来计划



\*\*当前限制\*\*：

\- 单关卡原型，没有关卡切换

\- 角色模型为静态，无骨骼动画

\- 没有 Post-Processing 后处理

\- 移动平台用速度合并方案，极端情况下可能不完美



\*\*未来计划\*\*：

\- 加入角色骨骼动画（Idle / Run / Jump / Fall）

\- 多关卡切换系统

\- Post-Processing（Bloom + Vignette）

\- 数据驱动的关卡设计（ScriptableObject 定义关卡）

\- 更多敌人类型（飞行敌人、跳跃敌人）

\- 排行榜 / 计时器记录最佳成绩



\---



\## 联系方式



\- GitHub: \[@chengyigg](https://github.com/chengyigg)

\- 邮箱：3201663235@qq.com

