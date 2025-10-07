import React, { useState, useEffect } from "react";
import { useAuth } from "../hooks/useAuth";
import { todoAPI } from "../services/api";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Checkbox } from "../components/ui/checkbox";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "../components/ui/card";
import { Modal } from "../components/ui/modal";
import Header from "./Header";
import {
  Plus,
  Trash2,
  Edit2,
  Save,
  X,
  Calendar,
  Clock,
  Search,
  Filter,
  CheckCircle,
  Circle,
  AlertCircle,
} from "lucide-react";

const TodoApp = () => {
  const { user, logout } = useAuth();
  const [todos, setTodos] = useState([]);
  const [newTodo, setNewTodo] = useState("");
  const [newTodoDescription, setNewTodoDescription] = useState("");
  const [newTodoDueDate, setNewTodoDueDate] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isAddingTodo, setIsAddingTodo] = useState(false);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [editingTodo, setEditingTodo] = useState(null);
  const [editText, setEditText] = useState("");
  const [editDescription, setEditDescription] = useState("");
  const [editDueDate, setEditDueDate] = useState("");

  // Search & Filter states
  const [searchQuery, setSearchQuery] = useState("");
  const [filterStatus, setFilterStatus] = useState("all"); // 'all', 'active', 'completed'
  const [filterDueDate, setFilterDueDate] = useState("all"); // 'all', 'overdue', 'today', 'upcoming', 'no-date'

  // Delete confirmation state
  const [todoToDelete, setTodoToDelete] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    fetchTodos();
  }, []);

  const fetchTodos = async () => {
    try {
      setIsLoading(true);
      const response = await todoAPI.getTodos();
      if (response.success) {
        // Sort todos by priority: overdue -> due today -> future -> no due date
        const sortedTodos = (response.data || []).sort((a, b) => {
          // Completed todos go to bottom
          if (a.isCompleted !== b.isCompleted) {
            return a.isCompleted ? 1 : -1;
          }

          // If both have no due date
          if (!a.dueDate && !b.dueDate) return 0;

          // No due date goes to bottom (among incomplete todos)
          if (!a.dueDate) return 1;
          if (!b.dueDate) return -1;

          // Sort by due date (earlier dates first)
          return new Date(a.dueDate) - new Date(b.dueDate);
        });

        setTodos(sortedTodos);
      }
    } catch (error) {
      console.error("Failed to fetch todos:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenAddModal = () => {
    setIsAddModalOpen(true);
  };

  const handleCloseAddModal = () => {
    setIsAddModalOpen(false);
    setNewTodo("");
    setNewTodoDescription("");
    setNewTodoDueDate("");
  };

  const handleAddTodo = async (e) => {
    e.preventDefault();

    if (!newTodo.trim()) return;

    try {
      setIsAddingTodo(true);
      const response = await todoAPI.createTodo({
        title: newTodo.trim(),
        description: newTodoDescription.trim(),
        dueDate: newTodoDueDate ? new Date(newTodoDueDate).toISOString() : null,
      });

      if (response.success) {
        setTodos((prev) => [...prev, response.data]);
        handleCloseAddModal();
      }
    } catch (error) {
      console.error("Failed to add todo:", error);
    } finally {
      setIsAddingTodo(false);
    }
  };

  const handleToggleTodo = async (id) => {
    try {
      const response = await todoAPI.toggleComplete(id);
      if (response.success) {
        setTodos((prev) =>
          prev.map((todo) =>
            todo.id === id ? { ...todo, isCompleted: !todo.isCompleted } : todo
          )
        );
      }
    } catch (error) {
      console.error("Failed to toggle todo:", error);
    }
  };

  const handleDeleteClick = (todo) => {
    setTodoToDelete(todo);
  };

  const handleCancelDelete = () => {
    setTodoToDelete(null);
  };

  const handleConfirmDelete = async () => {
    if (!todoToDelete) return;

    try {
      setIsDeleting(true);
      const response = await todoAPI.deleteTodo(todoToDelete.id);
      if (response.success) {
        setTodos((prev) => prev.filter((todo) => todo.id !== todoToDelete.id));
        setTodoToDelete(null);
      }
    } catch (error) {
      console.error("Failed to delete todo:", error);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleEditTodo = (todo) => {
    setEditingTodo(todo.id);
    setEditText(todo.title);
    setEditDescription(todo.description || "");
    // Format date for input field (YYYY-MM-DD)
    setEditDueDate(todo.dueDate ? todo.dueDate.split("T")[0] : "");
  };

  const handleCancelEdit = () => {
    setEditingTodo(null);
    setEditText("");
    setEditDescription("");
    setEditDueDate("");
  };

  const handleSaveEdit = async (id) => {
    if (!editText.trim()) return;

    try {
      const response = await todoAPI.updateTodo(id, {
        title: editText.trim(),
        description: editDescription.trim(),
        dueDate: editDueDate ? new Date(editDueDate).toISOString() : null,
      });

      if (response.success) {
        setTodos((prev) =>
          prev.map((todo) =>
            todo.id === id
              ? {
                  ...todo,
                  title: editText.trim(),
                  description: editDescription.trim(),
                  dueDate: editDueDate
                    ? new Date(editDueDate).toISOString()
                    : null,
                }
              : todo
          )
        );
        handleCancelEdit();
      }
    } catch (error) {
      console.error("Failed to update todo:", error);
    }
  };

  const completedCount = todos.filter((todo) => todo.isCompleted).length;
  const totalCount = todos.length;

  // Helper functions
  const formatDate = (dateString) => {
    if (!dateString) return null;
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year:
        date.getFullYear() !== new Date().getFullYear() ? "numeric" : undefined,
    });
  };

  const isOverdue = (dueDate) => {
    if (!dueDate) return false;
    const due = new Date(dueDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    due.setHours(0, 0, 0, 0);
    return due < today;
  };

  const isDueToday = (dueDate) => {
    if (!dueDate) return false;
    const due = new Date(dueDate);
    const today = new Date();
    return due.toDateString() === today.toDateString();
  };

  // Filter and search logic
  const filteredTodos = todos.filter((todo) => {
    // Search filter
    const matchesSearch =
      searchQuery === "" ||
      todo.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (todo.description &&
        todo.description.toLowerCase().includes(searchQuery.toLowerCase()));

    // Status filter
    let matchesStatus = true;
    if (filterStatus === "active") {
      matchesStatus = !todo.isCompleted;
    } else if (filterStatus === "completed") {
      matchesStatus = todo.isCompleted;
    }

    // Due date filter
    let matchesDueDate = true;
    if (filterDueDate === "overdue") {
      matchesDueDate =
        !todo.isCompleted && todo.dueDate && isOverdue(todo.dueDate);
    } else if (filterDueDate === "today") {
      matchesDueDate =
        !todo.isCompleted && todo.dueDate && isDueToday(todo.dueDate);
    } else if (filterDueDate === "upcoming") {
      matchesDueDate =
        !todo.isCompleted &&
        todo.dueDate &&
        !isOverdue(todo.dueDate) &&
        !isDueToday(todo.dueDate);
    } else if (filterDueDate === "no-date") {
      matchesDueDate = !todo.dueDate;
    }

    return matchesSearch && matchesStatus && matchesDueDate;
  });

  // Clear filters function
  const clearFilters = () => {
    setSearchQuery("");
    setFilterStatus("all");
    setFilterDueDate("all");
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Header
        user={user}
        logout={logout}
        completedCount={completedCount}
        totalCount={totalCount}
      />

      {/* Main Content */}
      <main className="max-w-4xl mx-auto px-4 py-8">
        {/* Statistics */}
        {todos.length > 0 && (
          <Card className="mb-6">
            <CardContent className="pt-6">
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
                <div>
                  <div className="text-2xl font-bold text-blue-600">
                    {todos.filter((t) => !t.isCompleted).length}
                  </div>
                  <div className="text-xs text-gray-600">Active</div>
                </div>
                <div>
                  <div className="text-2xl font-bold text-green-600">
                    {todos.filter((t) => t.isCompleted).length}
                  </div>
                  <div className="text-xs text-gray-600">Completed</div>
                </div>
                <div>
                  <div className="text-2xl font-bold text-red-600">
                    {
                      todos.filter(
                        (t) =>
                          !t.isCompleted && t.dueDate && isOverdue(t.dueDate)
                      ).length
                    }
                  </div>
                  <div className="text-xs text-gray-600">Overdue</div>
                </div>
                <div>
                  <div className="text-2xl font-bold text-orange-600">
                    {
                      todos.filter(
                        (t) =>
                          !t.isCompleted && t.dueDate && isDueToday(t.dueDate)
                      ).length
                    }
                  </div>
                  <div className="text-xs text-gray-600">Due Today</div>
                </div>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Search & Filters */}
        {todos.length > 0 && (
          <Card className="mb-6">
            <CardContent className="pt-6 space-y-4">
              {/* Search Bar */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                <Input
                  placeholder="Search todos..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10 pr-10"
                />
                {searchQuery && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => setSearchQuery("")}
                    className="absolute right-2 top-1/2 transform -translate-y-1/2 h-6 w-6 p-0 text-gray-400 hover:text-gray-600"
                  >
                    <X className="h-4 w-4" />
                  </Button>
                )}
              </div>

              {/* Filter Buttons */}
              <div className="flex flex-wrap gap-2">
                {/* Status Filters */}
                <div className="flex items-center space-x-1 ml-2 gap-1">
                  <Filter className="h-4 w-4 text-gray-500" />
                  <span className="text-sm text-gray-600 mr-2">Status:</span>
                  <Button
                    variant={filterStatus === "all" ? "default" : "outline"}
                    size="sm"
                    onClick={() => setFilterStatus("all")}
                    className="h-7"
                  >
                    All ({todos.length})
                  </Button>
                  <Button
                    variant={filterStatus === "active" ? "default" : "outline"}
                    size="sm"
                    onClick={() => setFilterStatus("active")}
                    className="h-7 flex items-center space-x-1"
                  >
                    <Circle className="h-3 w-3" />
                    <span>
                      Active ({todos.filter((t) => !t.isCompleted).length})
                    </span>
                  </Button>
                  <Button
                    variant={
                      filterStatus === "completed" ? "default" : "outline"
                    }
                    size="sm"
                    onClick={() => setFilterStatus("completed")}
                    className="h-7 flex items-center space-x-1"
                  >
                    <CheckCircle className="h-3 w-3" />
                    <span>
                      Completed ({todos.filter((t) => t.isCompleted).length})
                    </span>
                  </Button>
                </div>

                {/* Due Date Filters */}
                <div className="flex items-center space-x-1 ml-2 gap-1">
                  <Clock className="h-4 w-4 text-gray-500" />
                  <span className="text-sm text-gray-600 mr-2">Due:</span>
                  <Button
                    variant={filterDueDate === "all" ? "default" : "outline"}
                    size="sm"
                    onClick={() => setFilterDueDate("all")}
                    className="h-7"
                  >
                    All ({todos.length})
                  </Button>
                  <Button
                    variant={
                      filterDueDate === "overdue" ? "default" : "outline"
                    }
                    size="sm"
                    onClick={() => setFilterDueDate("overdue")}
                    className="h-7 flex items-center space-x-1 text-red-600 border-red-200 hover:bg-red-50"
                  >
                    <AlertCircle className="h-3 w-3" />
                    <span>
                      Overdue (
                      {
                        todos.filter(
                          (t) =>
                            !t.isCompleted && t.dueDate && isOverdue(t.dueDate)
                        ).length
                      }
                      )
                    </span>
                  </Button>
                  <Button
                    variant={filterDueDate === "today" ? "default" : "outline"}
                    size="sm"
                    onClick={() => setFilterDueDate("today")}
                    className="h-7 text-orange-600 border-orange-200 hover:bg-orange-50"
                  >
                    Today (
                    {
                      todos.filter(
                        (t) =>
                          !t.isCompleted && t.dueDate && isDueToday(t.dueDate)
                      ).length
                    }
                    )
                  </Button>
                  <Button
                    variant={
                      filterDueDate === "upcoming" ? "default" : "outline"
                    }
                    size="sm"
                    onClick={() => setFilterDueDate("upcoming")}
                    className="h-7"
                  >
                    Upcoming (
                    {
                      todos.filter(
                        (t) =>
                          !t.isCompleted &&
                          t.dueDate &&
                          !isOverdue(t.dueDate) &&
                          !isDueToday(t.dueDate)
                      ).length
                    }
                    )
                  </Button>
                  <Button
                    variant={
                      filterDueDate === "no-date" ? "default" : "outline"
                    }
                    size="sm"
                    onClick={() => setFilterDueDate("no-date")}
                    className="h-7"
                  >
                    No Date ({todos.filter((t) => !t.dueDate).length})
                  </Button>
                </div>

                {/* Clear Filters */}
                {(searchQuery ||
                  filterStatus !== "all" ||
                  filterDueDate !== "all") && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={clearFilters}
                    className="h-7 text-gray-500 hover:text-gray-700 ml-4"
                  >
                    Clear All
                  </Button>
                )}
              </div>

              {/* Active Filters Summary */}
              {(searchQuery ||
                filterStatus !== "all" ||
                filterDueDate !== "all") && (
                <div className="flex items-center space-x-2 text-sm text-gray-600 pt-2 border-t">
                  <span>
                    Showing {filteredTodos.length} of {todos.length} todos
                  </span>
                  {searchQuery && (
                    <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs">
                      Search: "{searchQuery}"
                    </span>
                  )}
                  {filterStatus !== "all" && (
                    <span className="px-2 py-1 bg-green-100 text-green-800 rounded-full text-xs">
                      Status: {filterStatus}
                    </span>
                  )}
                  {filterDueDate !== "all" && (
                    <span className="px-2 py-1 bg-purple-100 text-purple-800 rounded-full text-xs">
                      Due: {filterDueDate}
                    </span>
                  )}
                </div>
              )}
            </CardContent>
          </Card>
        )}

        {/* Quick Add Button (when no todos) */}
        {todos.length === 0 && (
          <div className="mb-8">
            <Button
              onClick={handleOpenAddModal}
              className="w-full h-16 flex items-center justify-center space-x-3 text-lg border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-gray-400 text-gray-600 hover:text-gray-800 transition-colors"
              variant="ghost"
            >
              <Plus className="h-6 w-6" />
              <span>Add your first todo</span>
            </Button>
          </div>
        )}

        {/* Todo List */}
        <div className="space-y-2">
          {isLoading ? (
            <Card>
              <CardContent className="py-8 text-center">
                <p className="text-gray-500">Loading todos...</p>
              </CardContent>
            </Card>
          ) : todos.length === 0 ? (
            <Card>
              <CardContent className="py-8 text-center">
                <p className="text-gray-500 mb-2">No todos yet!</p>
                <p className="text-sm text-gray-400">
                  Add your first todo above to get started.
                </p>
              </CardContent>
            </Card>
          ) : filteredTodos.length === 0 ? (
            <Card>
              <CardContent className="py-8 text-center">
                <p className="text-gray-500 mb-2">
                  No todos match your filters
                </p>
                <p className="text-sm text-gray-400 mb-4">
                  Try adjusting your search or filter criteria.
                </p>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={clearFilters}
                  className="flex items-center space-x-2"
                >
                  <X className="h-4 w-4" />
                  <span>Clear Filters</span>
                </Button>
              </CardContent>
            </Card>
          ) : (
            filteredTodos.map((todo) => (
              <Card key={todo.id} className="transition-shadow hover:shadow-md">
                <CardContent className="py-4">
                  {editingTodo === todo.id ? (
                    // Edit Mode
                    <div className="space-y-3">
                      <div className="flex items-center space-x-3">
                        <Checkbox
                          checked={todo.isCompleted}
                          onCheckedChange={() => handleToggleTodo(todo.id)}
                        />
                        <Input
                          value={editText}
                          onChange={(e) => setEditText(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") {
                              e.preventDefault();
                              handleSaveEdit(todo.id);
                            } else if (e.key === "Escape") {
                              handleCancelEdit();
                            }
                          }}
                          placeholder="Todo title..."
                          className="flex-1"
                          autoFocus
                        />
                      </div>

                      <div className="ml-8 space-y-2">
                        <Input
                          value={editDescription}
                          onChange={(e) => setEditDescription(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") {
                              e.preventDefault();
                              handleSaveEdit(todo.id);
                            } else if (e.key === "Escape") {
                              handleCancelEdit();
                            }
                          }}
                          placeholder="Description (optional)..."
                          className="w-full"
                        />

                        <div className="flex items-center space-x-2">
                          <Input
                            type="date"
                            value={editDueDate}
                            onChange={(e) => setEditDueDate(e.target.value)}
                            className="w-auto"
                            placeholder="Due date (optional)"
                          />
                          {editDueDate && (
                            <Button
                              type="button"
                              variant="ghost"
                              size="sm"
                              onClick={() => setEditDueDate("")}
                              className="text-gray-500 hover:text-gray-700"
                            >
                              <X className="h-3 w-3" />
                            </Button>
                          )}
                        </div>
                      </div>

                      <div className="flex items-center justify-end space-x-2 ml-8">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={handleCancelEdit}
                          className="text-gray-600 hover:text-gray-800"
                        >
                          <X className="h-4 w-4 mr-1" />
                          Cancel
                        </Button>
                        <Button
                          size="sm"
                          onClick={() => handleSaveEdit(todo.id)}
                          disabled={!editText.trim()}
                          className="text-white bg-blue-600 hover:bg-blue-700"
                        >
                          <Save className="h-4 w-4 mr-1" />
                          Save
                        </Button>
                      </div>
                    </div>
                  ) : (
                    // View Mode
                    <div className="flex items-center space-x-3">
                      <Checkbox
                        checked={todo.isCompleted}
                        onCheckedChange={() => handleToggleTodo(todo.id)}
                      />

                      <div
                        className="flex-1 min-w-0 cursor-pointer hover:bg-gray-50 rounded px-2 py-1 transition-colors"
                        onDoubleClick={() => handleEditTodo(todo)}
                        title="Double-click to edit"
                      >
                        <p
                          className={`text-sm font-medium ${
                            todo.isCompleted
                              ? "line-through text-gray-500"
                              : "text-gray-900"
                          }`}
                        >
                          {todo.title}
                        </p>

                        {todo.description && (
                          <p
                            className={`text-xs mt-1 ${
                              todo.isCompleted
                                ? "line-through text-gray-400"
                                : "text-gray-600"
                            }`}
                          >
                            {todo.description}
                          </p>
                        )}

                        {todo.dueDate && (
                          <div
                            className={`flex items-center space-x-1 mt-1 ${
                              todo.isCompleted
                                ? "text-gray-400"
                                : isOverdue(todo.dueDate)
                                ? "text-red-600"
                                : isDueToday(todo.dueDate)
                                ? "text-orange-600"
                                : "text-gray-500"
                            }`}
                          >
                            <Clock className="h-3 w-3" />
                            <span className="text-xs">
                              {isDueToday(todo.dueDate)
                                ? "Due today"
                                : isOverdue(todo.dueDate)
                                ? `Overdue (${formatDate(todo.dueDate)})`
                                : `Due ${formatDate(todo.dueDate)}`}
                            </span>
                          </div>
                        )}
                      </div>

                      <div className="flex items-center space-x-1">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEditTodo(todo)}
                          className="text-blue-600 hover:text-blue-800 hover:bg-blue-50"
                          title="Edit todo"
                        >
                          <Edit2 className="h-4 w-4" />
                        </Button>

                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDeleteClick(todo)}
                          className="text-red-600 hover:text-red-800 hover:bg-red-50"
                          title="Delete todo"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  )}
                </CardContent>
              </Card>
            ))
          )}
        </div>

        {/* Stats */}
        {todos.length > 0 && (
          <Card className="mt-8">
            <CardHeader>
              <CardTitle className="text-lg">Progress</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between text-sm">
                <span className="text-gray-600">Total: {totalCount}</span>
                <span className="text-gray-600">
                  Completed: {completedCount}
                </span>
                <span className="text-gray-600">
                  Remaining: {totalCount - completedCount}
                </span>
              </div>

              <div className="mt-3 bg-gray-200 rounded-full h-2">
                <div
                  className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                  style={{
                    width: `${
                      totalCount > 0 ? (completedCount / totalCount) * 100 : 0
                    }%`,
                  }}
                />
              </div>

              <p className="text-center text-sm text-gray-600 mt-2">
                {totalCount > 0
                  ? Math.round((completedCount / totalCount) * 100)
                  : 0}
                % Complete
              </p>
            </CardContent>
          </Card>
        )}
      </main>

      {/* Floating Action Button */}
      <Button
        onClick={handleOpenAddModal}
        className="fixed bottom-8 right-8 h-14 w-14 rounded-full shadow-lg hover:shadow-xl transition-all duration-200 bg-blue-600 hover:bg-blue-700 z-40"
        title="Add new todo"
      >
        <Plus className="h-6 w-6 text-white" />
      </Button>

      {/* Delete Confirmation Modal */}
      <Modal
        isOpen={!!todoToDelete}
        onClose={handleCancelDelete}
        title="Delete Todo"
        size="sm"
      >
        <div className="space-y-4">
          <div className="flex items-center space-x-3">
            <div className="flex-shrink-0">
              <div className="w-10 h-10 bg-red-100 rounded-full flex items-center justify-center">
                <Trash2 className="w-5 h-5 text-red-600" />
              </div>
            </div>
            <div className="flex-1">
              <h3 className="text-lg font-medium text-gray-900">
                Are you sure?
              </h3>
              <p className="text-sm text-gray-500 mt-1">
                This action cannot be undone. This will permanently delete the
                todo:
              </p>
            </div>
          </div>

          {todoToDelete && (
            <div className="bg-gray-50 rounded-lg p-3 border-l-4 border-gray-300">
              <p className="font-medium text-gray-900">{todoToDelete.title}</p>
              {todoToDelete.description && (
                <p className="text-sm text-gray-600 mt-1">
                  {todoToDelete.description}
                </p>
              )}
              {todoToDelete.dueDate && (
                <div className="flex items-center space-x-1 mt-2 text-xs text-gray-500">
                  <Clock className="h-3 w-3" />
                  <span>Due: {formatDate(todoToDelete.dueDate)}</span>
                </div>
              )}
            </div>
          )}

          <div className="flex justify-end space-x-3 pt-4 border-t">
            <Button
              variant="outline"
              onClick={handleCancelDelete}
              disabled={isDeleting}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleConfirmDelete}
              disabled={isDeleting}
              className="flex items-center space-x-2"
            >
              <Trash2 className="h-4 w-4" />
              <span>{isDeleting ? "Deleting..." : "Delete Todo"}</span>
            </Button>
          </div>
        </div>
      </Modal>

      {/* Add Todo Modal */}
      <Modal
        isOpen={isAddModalOpen}
        onClose={handleCloseAddModal}
        title="Add New Todo"
        size="md"
      >
        <form onSubmit={handleAddTodo} className="space-y-4">
          <div>
            <label
              htmlFor="todoTitle"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Title *
            </label>
            <Input
              id="todoTitle"
              placeholder="What needs to be done?"
              value={newTodo}
              onChange={(e) => setNewTodo(e.target.value)}
              className="w-full"
              disabled={isAddingTodo}
              autoFocus
            />
          </div>

          <div>
            <label
              htmlFor="todoDescription"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Description
            </label>
            <Input
              id="todoDescription"
              placeholder="Additional details (optional)"
              value={newTodoDescription}
              onChange={(e) => setNewTodoDescription(e.target.value)}
              className="w-full"
              disabled={isAddingTodo}
            />
          </div>

          <div>
            <label
              htmlFor="todoDueDate"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Due Date
            </label>
            <div className="flex items-center space-x-2">
              <Input
                id="todoDueDate"
                type="date"
                value={newTodoDueDate}
                onChange={(e) => setNewTodoDueDate(e.target.value)}
                className="flex-1"
                disabled={isAddingTodo}
              />
              {newTodoDueDate && (
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => setNewTodoDueDate("")}
                  className="text-gray-500 hover:text-gray-700"
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
            </div>
          </div>

          <div className="flex justify-end space-x-2 pt-4 border-t">
            <Button
              type="button"
              variant="outline"
              onClick={handleCloseAddModal}
              disabled={isAddingTodo}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={isAddingTodo || !newTodo.trim()}
              className="flex items-center space-x-2"
            >
              <Plus className="h-4 w-4" />
              <span>{isAddingTodo ? "Adding..." : "Add Todo"}</span>
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default TodoApp;
