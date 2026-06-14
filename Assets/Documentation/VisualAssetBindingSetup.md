# 视觉素材绑定设置说明

## 概述

本文档提供了如何将已导入的视觉素材绑定到当前 UI 与场景对象的详细步骤。

## 已创建的文件

1. **VisualAssetBinder.cs** - 用于绑定按钮素材和列车占位图像
2. **SceneSetup.cs** - 用于设置 tilemap 和生成列车对象
3. **TrainPlaceholder.prefab** - 列车占位预制体

## 设置步骤

### 1. 导入素材

确保以下素材已导入到项目中：

- `Assets/Art/UI/Prototype/button_primary.png`
- `Assets/Art/UI/Prototype/button_secondary.png`
- `Assets/Art/Vehicles/Prototype/train_placeholder.png`
- `Assets/Art/Environment/Tilesets/tiny-town_tilemap_packed.png`

### 2. 创建 Tilemap

1. 在场景中创建一个新的 Tilemap 对象：
   - 右键点击 Hierarchy 面板
   - 选择 `2D Object` → `Tilemap` → `Rectangular`
   - 将其命名为 "GroundTilemap"

2. 创建 Tile Palette：
   - 打开 Window → 2D → Tile Palette
   - 创建一个新的 Tile Palette，命名为 "TinyTown"
   - 选择 `Assets/Art/Environment/Tilesets/tiny-town_tilemap_packed.png` 作为纹理
   - 调整单元格大小为合适的值（例如 32x32）
   - 将纹理分割成多个瓦片

3. 为 SceneSetup 脚本设置 tilemap：
   - 在场景中创建一个空对象，命名为 "SceneSetup"
   - 将 `SceneSetup.cs` 脚本添加到该对象
   - 在 Inspector 面板中，将 "GroundTilemap" 拖放到 `groundTilemap` 字段
   - 从 Tile Palette 中选择一个瓦片拖放到 `groundTile` 字段

### 3. 设置列车对象

1. 在场景中创建列车对象：
   - 使用 `TrainPlaceholder.prefab` 预制体，或
   - 创建一个新的空对象，命名为 "Train"
   - 添加 SpriteRenderer 组件
   - 将 `train_placeholder.png` 拖放到 Sprite 字段

2. 为 SceneSetup 脚本设置列车预制体：
   - 将列车对象保存为预制体，命名为 "TrainPlaceholder"
   - 在 SceneSetup 对象的 Inspector 面板中，将该预制体拖放到 `trainPrefab` 字段

### 4. 绑定按钮素材

1. 在场景中找到底部的按钮对象：
   - 通常命名为 "ScheduleButton", "StaffButton", "MaintenanceButton", "StationServiceButton", "ExternalAffairsButton", "EndDayButton"

2. 创建视觉素材绑定器：
   - 在场景中创建一个空对象，命名为 "VisualAssetBinder"
   - 将 `VisualAssetBinder.cs` 脚本添加到该对象

3. 在 Inspector 面板中设置：
   - 将 `button_primary.png` 拖放到 `buttonPrimarySprite` 字段
   - 将 `button_secondary.png` 拖放到 `buttonSecondarySprite` 字段
   - 将 `train_placeholder.png` 拖放到 `trainPlaceholderSprite` 字段
   - 将各个按钮对象拖放到对应的字段：
     - `scheduleButton`
     - `staffButton`
     - `maintenanceButton`
     - `stationServiceButton`
     - `externalAffairsButton`
     - `endDayButton`
   - 将列车对象拖放到 `trainObject` 字段
   - 将 tilemap 对象拖放到 `tilemapObject` 字段

### 5. 测试设置

1. 运行场景，检查：
   - 底部按钮是否显示为导入的按钮素材
   - 场景中是否显示 tilemap 地面
   - 场景中是否显示列车占位图像

## 注意事项

- 确保所有素材都已正确导入到项目中
- 确保脚本中的引用都已正确设置
- 如果按钮大小不合适，可能需要调整按钮的 RectTransform 组件
- 如果 tilemap 显示不正确，可能需要调整 tilemap 的设置
- 如果列车图像显示不正确，可能需要调整列车对象的缩放和位置

## 故障排除

- **按钮素材不显示**：检查按钮的 Image 组件是否启用，以及 sprite 是否正确设置
- **Tilemap 不显示**：检查 tilemap 是否处于激活状态，以及 tile 是否正确设置
- **列车不显示**：检查列车对象是否处于激活状态，以及 sprite 是否正确设置
