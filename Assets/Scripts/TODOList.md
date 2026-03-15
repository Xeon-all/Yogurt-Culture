# 编码过程遗留 TODO List

## 概述

本文档记录 Yogurt Culture 项目编码过程中遗留的待办事项，便于后续跟进处理。

---

## 待办事项

### 1. YogurtProcessData.cs - 标签列表处理

- **文件**: `YogurtGame/Order/YogurtProcessData.cs`
- **行号**: ~98
- **描述**: 根据 `AddedToppingTags` 返回完整的标签列表
- **优先级**: 中

---

### 2. OrderManager.cs - YogurtTag 替换

- **文件**: `YogurtGame/Order/OrderManager.cs`
- **行号**: ~78
- **描述**: 替换为实际的 `YogurtTag`（当前可能是占位符）
- **优先级**: 中

---

## 已完成事项

- [x] YogurtGameBoard 数据加载重构（移除 debug 测试代码）
- [x] ToppingData.Tags 解析支持（逗号分隔）
- [x] 未知 Tag 自动注册到 YogurtTag 枚举
