# Unity-Singleton
Over engineered Singleton base class for Unity

After creating first instance of it on the scene it will automatically create a prefab
being instantiated every time when reffered to, while there is no instance present. 

Remember to call base implementation of methods like: Awake, Enable/Disable, ApplicationQuit, Reset, when overriding them.
