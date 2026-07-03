bool isGameOptionTileDisabled({
  required bool screenDisabled,
  required bool optionIsActive,
  required bool optionIsPrevious,
}) {
  return screenDisabled || (!optionIsActive && !optionIsPrevious);
}
