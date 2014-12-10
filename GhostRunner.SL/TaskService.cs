﻿using GhostRunner.DAL;
using GhostRunner.DAL.Interface;
using GhostRunner.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GhostRunner.SL
{
    public class TaskService
    {
        #region Private Properties

        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IUserDataAccess _userDataAccess;
        private ISequenceDataAccess _sequenceDataAccess;
        private IScriptDataAccess _scriptDataAccess;
        private ISequenceScriptDataAccess _sequenceScriptDataAccess;
        private ITaskDataAccess _taskDataAccess;
        private ITaskParameterDataAccess _taskParameterDataAccess;

        #endregion

        #region Constructors

        public TaskService()
        {
            InitializeDataAccess(new GhostRunnerContext("DatabaseConnectionString"));
        }

        public TaskService(IContext context)
        {
            InitializeDataAccess(context);
        }

        #endregion

        #region Public Methods

        public IList<Task> GetAllTasks(int projectId)
        {
            return _taskDataAccess.GetAllByProjectId(projectId).OrderByDescending(it => it.Created).OrderBy(it => it.Status).ToList();
        }

        public Task InsertScriptTask(int userId, String scriptId, String name, IList<TaskParameter> taskParameters)
        {
            User user = _userDataAccess.GetById(userId);

            if (user != null)
            {
                Script script = _scriptDataAccess.Get(scriptId);

                if (script != null)
                {
                    Task task = new Task();
                    task.ExternalId = System.Guid.NewGuid().ToString();
                    task.ParentId = script.ID;
                    task.ParentType = ItemType.Script;
                    task.Name = name;
                    task.Content = script.Content;
                    task.Status = Status.Unprocessed;
                    task.Created = DateTime.UtcNow;

                    task = _taskDataAccess.Insert(task);

                    if (taskParameters != null)
                    {
                        foreach (TaskParameter scriptTaskParameter in taskParameters)
                        {
                            InsertTaskParameter(task.ExternalId, scriptTaskParameter.Name, scriptTaskParameter.Value);
                        }
                    }

                    return task;
                }
                else
                {
                    _log.Info("InsertScriptTask(" + scriptId + "): Unable to find script");

                    return null;
                }
            }
            else
            {
                _log.Info("InsertScriptTask(" + userId + "): Unable to find user");

                return null;
            }
        }

        public Task InsertSequenceTask(int userId, String sequenceId, String name)
        {
            User user = _userDataAccess.GetById(userId);

            if (user != null)
            {
                Sequence sequence = _sequenceDataAccess.Get(sequenceId);

                if (sequence != null)
                {
                    Task task = new Task();
                    task.ExternalId = System.Guid.NewGuid().ToString();
                    task.ParentId = sequence.ID;
                    task.ParentType = ItemType.Sequence;
                    task.Name = name;
                    task.Status = Status.Unprocessed;
                    task.Created = DateTime.UtcNow;

                    task = _taskDataAccess.Insert(task);

                    foreach (SequenceScript sequenceScript in sequence.SequenceScripts)
                    {
                        InsertTaskParameter(task.ExternalId, sequenceScript.Name, sequenceScript.Content);
                    }

                    return task;
                }
                else
                {
                    _log.Info("InsertSequenceTask(" + sequenceId + "): Unable to find script");

                    return null;
                }
            }
            else
            {
                _log.Info("InsertTask(" + userId + "): Unable to find user");

                return null;
            }
        }

        public Task InsertSequenceScriptTask(int userId, String sequenceScriptId)
        {
            User user = _userDataAccess.GetById(userId);

            if (user != null)
            {
                SequenceScript sequenceScript = _sequenceScriptDataAccess.Get(sequenceScriptId);

                if (sequenceScript != null)
                {
                    Task task = new Task();
                    task.ExternalId = System.Guid.NewGuid().ToString();
                    task.ParentId = sequenceScript.ID;
                    task.ParentType = ItemType.SequenceScript;
                    task.Name = sequenceScript.Name;
                    task.Content = sequenceScript.Content;
                    task.Status = Status.Unprocessed;
                    task.Created = DateTime.UtcNow;

                    return _taskDataAccess.Insert(task);
                }
                else
                {
                    _log.Info("InsertSequenceScriptTask(" + sequenceScriptId + "): Unable to find sequence script");

                    return null;
                }
            }
            else
            {
                _log.Info("InsertTask(" + userId + "): Unable to find user");

                return null;
            }
        }

        public TaskParameter InsertTaskParameter(String taskId, String name, String value)
        {
            Task task = _taskDataAccess.Get(taskId);

            if (task != null)
            {
                TaskParameter taskParameter = new TaskParameter();
                taskParameter.Task = task;
                taskParameter.Name = name;
                taskParameter.Value = value;

                return _taskParameterDataAccess.Insert(taskParameter);
            }
            else
            {
                _log.Info("InsertTaskParameter(" + taskId + "): Unable to find script task");

                return null;
            }
        }

        #endregion

        #region Private Methods

        private void InitializeDataAccess(IContext context)
        {
            _userDataAccess = new UserDataAccess(context);
            _sequenceDataAccess = new SequenceDataAccess(context);
            _scriptDataAccess = new ScriptDataAccess(context);
            _sequenceScriptDataAccess = new SequenceScriptDataAccess(context);
            _taskDataAccess = new TaskDataAccess(context);
            _taskParameterDataAccess = new TaskParameterDataAccess(context);
        }

        #endregion
    }
}