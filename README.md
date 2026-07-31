# CodeBind - Unity Hierarchy 绑定代码生成工具

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2019.4%2B-blue" alt="Unity Version"/>
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License"/>
  <img src="https://img.shields.io/badge/Language-C%23-brightgreen" alt="Language"/>
</p>

CodeBind 根据 Unity Hierarchy 节点名称收集目标，生成 partial C# 绑定代码并写入序列化引用。它支持直接绑定 `MonoBehaviour`、由 Unity Host 承载普通 C# 类，以及按字符串 key 访问引用。

## 核心特性

- 节点名称支持类型 token、缩写模糊匹配、多个目标和 `*` wildcard。
- 使用 `(0)`、`(1)` 等后缀生成数组成员。
- 支持嵌套 `BindingRootAttribute` 边界，避免父级重复收集子绑定根。
- 支持自定义名称分隔符、输出路径和命名空间。
- 通过 `IBindingTargetTokenConfig` 批量扩展 token 映射。
- 通过 `IBindingCodeCustomizer` 自定义生成字段、属性和附加源码。
- 生成结果使用 partial class，不修改业务实现。

## 安装

在 Unity Package Manager 中选择 **Add package from git URL**，输入：

```text
https://github.com/XuToWei/CodeBind.git
```

也可以下载仓库并放入项目的 `Packages` 目录。

### 依赖

CodeBind 需要 [Odin Inspector](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041)。

## Hierarchy 命名规则

默认格式：

```text
[变量名]_[目标类型 token][_目标类型 token...][ (数组索引)]
```

示例：

| 节点名称 | 生成成员 | 说明 |
|---|---|---|
| `Player_Transform` | `PlayerTransform` | 绑定 Transform |
| `UI_Button_Image` | `UIButton`、`UIImage` | 同一节点绑定多个目标 |
| `Panel_*` | `PanelGameObject`、`PanelTransform` 及该节点上其他已映射组件 | wildcard 使用实际目标 token，不生成 `PanelStar` |
| `Item_Transform (0)`、`Item_Transform (1)` | `ItemTransformArray` | 按索引后缀收集数组 |

类型 token 支持模糊补全，例如 `Self_Tr` 可规范化为 `Self_Transform`。常见内置 token 包括 `GameObject`、`Transform`、`RectTransform`、`Button`、`Text`、`Image` 和 `Slider`。

> CodeBind 直接将 `变量名 + 目标 token` 交给当前 `IBindingCodeCustomizer`。默认实现不会自动转换大小写；若希望生成 PascalCase，请在 Hierarchy 变量名或 customizer 中明确实现。

## MonoBehaviour 绑定

在业务脚本上添加 `MonoBehaviourBindingAttribute`：

```csharp
using CodeBind;
using UnityEngine;

namespace Game
{
    [MonoBehaviourBinding('_')]
    public partial class PlayerView : MonoBehaviour
    {
        private void Start()
        {
            RootTransform.position = Vector3.zero;
            SubmitButton.onClick.AddListener(Submit);
        }

        private void Submit()
        {
            Debug.Log("Submitted");
        }
    }
}
```

对应节点：

```text
PlayerView
├── Root_Transform
└── Submit_Button
```

生成的 `PlayerView.Bind.cs` 类似：

```csharp
namespace Game
{
    public partial class PlayerView
    {
        [UnityEngine.SerializeField]
        private UnityEngine.Transform m_RootTransform;

        [UnityEngine.SerializeField]
        private UnityEngine.UI.Button m_SubmitButton;

        public UnityEngine.Transform RootTransform => m_RootTransform;
        public UnityEngine.UI.Button SubmitButton => m_SubmitButton;
    }
}
```

在 Odin Inspector 中使用 **Generate Binding Source** 和 **Generate Serialization**。新建脚本也可以使用 Hierarchy 菜单：

```text
GameObject > CodeBind > MonoBehaviour Binding Generator
```

## PlainClass 绑定

PlainClass 模式适合不继承 `MonoBehaviour` 的普通 C# 类。业务主脚本只需要声明为 partial；生成的 `.Bind.cs` 会实现 `IPlainClassBinding`：

```csharp
namespace Game
{
    public partial class PlayerPresenter
    {
        public void Show()
        {
            TitleText.text = "Hello CodeBind";
        }
    }
}
```

1. 在 GameObject 上添加 `PlainClassBindingHost`。
2. 将业务脚本赋给 Host 的 **Binding Class Script**。
3. 生成绑定源码和序列化数据。
4. 通过扩展方法取得缓存实例：

```csharp
PlayerPresenter presenter = gameObject.GetPlainClassBinding<PlayerPresenter>();
presenter.Show();
```

生成协议提供：

```csharp
public PlainClassBindingHost BindingHost { get; private set; }
public Transform RootTransform { get; private set; }
public void Initialize(PlainClassBindingHost host);
public void Reset();
```

新建 PlainClass 脚本可以使用：

```text
GameObject > CodeBind > Plain Class Binding Generator
```

## NamedReference 绑定

`NamedReferenceBindingHost` 支持两类引用：

- Inspector 手动维护的 key 到 `GameObject` 映射。
- 根据 Hierarchy 命名自动生成的 key 到目标映射。

运行时 API：

```csharp
GameObject panel = host.GetManualGameObject("Panel");
Button submit = host.GetAutoTarget<Button>("SubmitButton");
List<Image> icons = host.GetAutoTargets<Image>("IconImageArray");
```

## 自定义 BindingTargetToken

对于单个自定义 `Component`，可以直接标记 token：

```csharp
using CodeBind;
using UnityEngine;

[BindingTargetToken("HealthBar")]
public sealed class HealthBar : MonoBehaviour
{
}
```

需要批量配置、别名或 `GameObject` 映射时，实现 `IBindingTargetTokenConfig`：

```csharp
using System;
using System.Collections.Generic;
using CodeBind.Editor;

public sealed class ProjectBindingTargetTokenConfig : IBindingTargetTokenConfig
{
    public int Priority => 100;

    public IReadOnlyDictionary<string, Type> TargetTypesByToken { get; } =
        new Dictionary<string, Type>
        {
            { "HealthBar", typeof(HealthBar) },
            { "HB", typeof(HealthBar) },
        };
}
```

发现优先级为：`BindingTargetTokenAttribute` > `IBindingTargetTokenConfig`（按 `Priority` 从高到低）> 默认配置。

## 自定义生成代码

实现 `IBindingCodeCustomizer` 即可自定义生成成员名称和附加源码。最高 `Priority` 的实现生效；默认实现优先级为 `0`。

```csharp
using System.Collections.Generic;
using CodeBind.Editor;

public sealed class ProjectBindingCodeCustomizer : IBindingCodeCustomizer
{
    public int Priority => 100;

    public string GetSerializedFieldName(string memberName) => $"_{memberName}";

    public string GetPublicPropertyName(string memberName) => memberName;

    public string BuildAdditionalSource(
        string namespaceName,
        string className,
        List<BindingDescriptor> singleBindings,
        SortedDictionary<string, List<BindingDescriptor>> arrayBindingsByMemberName,
        string indentation)
    {
        return string.Empty;
    }
}
```

框架传入的 `memberName` 已经是 `VariableName + TargetToken`；数组还会预先追加固定的 `Array` 后缀。不要在 customizer 中再次拼接 token 或数组后缀。

`BindingDescriptor` 提供：

```csharp
public string VariableName { get; }
public Type TargetType { get; }
public string TargetToken { get; }
public Transform SourceTransform { get; }
```

## 批量刷新

选中一个或多个根 GameObject 后，可以使用：

```text
GameObject > CodeBind > Refresh All Binding Sources
GameObject > CodeBind > Refresh All Serialized Bindings
```

## 常见问题

### 生成文件在哪里？

已有脚本的生成文件位于原脚本旁边，名称为 `原类名.Bind.cs`。Generator Window 使用所选输出目录。

### 修改 Hierarchy 后需要重新生成吗？

需要。生成绑定源码后，再生成序列化数据。MonoBehaviour Inspector 在脚本重载后会自动完成待处理的序列化步骤。

### 支持嵌套绑定吗？

支持。带 `BindingRootAttribute` 的组件会形成扫描边界，父绑定不会收集嵌套绑定根内部的节点。

## 交流与反馈

- **QQ 交流群**：949482664
- **问题反馈**：[GitHub Issues](https://github.com/XuToWei/CodeBind/issues)

## License

本项目基于 [MIT License](LICENSE) 开源。
