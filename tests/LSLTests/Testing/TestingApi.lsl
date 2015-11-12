//#!Enable:Testing

default
{
	state_entry()
	{
		_test_Log(LOG_INFO, "Info");
		_test_Log(LOG_WARN, "Warn");
		_test_Log(LOG_ERROR, "Error");
		_test_Log(LOG_FATAL, "Fatal");
		_test_Log(LOG_DEBUG, "Debug");
		_test_Result(1);
		_test_Shutdown();
	}
}
