# Collection locks

Each collection has independently persisted position and size locks. Position locking disables header dragging; size locking disables the manual resize grip. Both toggles are accessible from the collection footer and stored in workspace-v1.json. Existing workspaces deserialize both flags as false. This is a first pass; external Windows window-management tools may still move/resize the window.
