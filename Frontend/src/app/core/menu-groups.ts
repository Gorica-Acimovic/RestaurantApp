import { MenuGroup, MenuItemDto } from './models/menu.models';

export function groupMenuItems(items: MenuItemDto[]): MenuGroup[] {
  const groups: MenuGroup[] = [];

  for (const item of items) {
    let group = groups.find(g => g.subcategoryId === item.subcategoryId);

    if (!group) {
      group = { subcategoryId: item.subcategoryId, name: item.subcategoryName, category: item.category, items: [] };
      groups.push(group);
    }

    group.items.push(item);
  }

  return groups;
}