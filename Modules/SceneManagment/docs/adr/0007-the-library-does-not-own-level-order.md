# The library does not decide level order

<!-- layout new -->

<!-- decision -->

The scene system changes to the scene group it is given. It has no level list, "next level" or stage order.

## The decision

- **Who orders levels:** each game's level manager, because a rally game, a survivors-like and a horror game order their places differently.
- **The library's job:** make each scene group quick to create, put its scenes in the build or Addressables automatically, and make changing to it one call.
