## 2024-04-04 - Dynamic Button Text in WPF
**Learning:** When using a DataTrigger in a WPF Style to change a button's Content dynamically, setting the default Content property directly on the Button tag (e.g., `<Button Content="Start Batch">`) will override the trigger due to dependency property precedence rules.
**Action:** Always move the default Content setting into a Style Setter (e.g., `<Setter Property="Content" Value="Start Batch" />`) alongside the DataTrigger so that state changes can successfully override it.
