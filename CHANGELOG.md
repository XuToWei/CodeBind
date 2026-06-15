# Changelog

## [1.1.0] - 2026-6-15
- 添加ICodeBindCustomizer接口，支持自定义命名风格和额外代码生成，带优先级，高优先级覆盖低优先级
- DefaultCodeBindNameTypeConfig改为ICodeBindNameTypeConfig接口实现，支持多实现按优先级合并覆盖
- 移除CodeBindNameTypeAttribute，由ICodeBindNameTypeConfig替代
- 移除内置STATE_CONTROLLER_CODE_BIND支持，可通过ICodeBindCustomizer自行实现
- 统一日志为英文并添加[CodeBind]前缀

## [1.0.7] - 2025-1-22
- 添加Mono类型自动绑定引用

## [1.0.6] - 2024-5-28
- ReferenceBindMono添加自动数据绑定

## [1.0.5] - 2024-3-15
- 添加STATE_CONTROLLER_CODE_BIND可以支持StateController数据代码生成
- 优化类型收集调用时机，降低ReloadDomain耗时

## [1.0.4] - 2024-3-13
- 支持继承的名称准确识别

## [1.0.3] - 2024-3-9
- fix the problem of nested judgment error

## [1.0.2] - 2024-1-26
- change name style

## [1.0.1] - 2024-1-20
- support bind gameObject
- fix binding partially duplicated components, the name may be modified.
- add bind editor group view.

## [1.0.0] - 2023-10-18