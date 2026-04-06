## 2024-05-24 - Overriding Button Content in WPF Styles
**Learning:** In WPF, if a UI element (like a Button) has a property set directly on the XML tag (e.g., `Content="Start Batch"`), it overrides any `DataTrigger` in a `Style` trying to change that same property.
**Action:** When using a `Style`'s `DataTrigger` to dynamically update button text (like showing "Running..."), set the default text using a `Setter` inside the `Style` instead of directly on the element tag. Also avoid mixing `BasedOn` static resources with inline definitions if the resource does not exist.
