import React from "react";
import { Button } from "./ui/button";
import { User, LogOut } from "lucide-react";

const Header = ({ user, logout }) => {
  return (
    <header className="bg-white shadow-sm border-b sticky top-0 z-40">
      <div className="max-w-4xl mx-auto px-3 sm:px-4 py-3 sm:py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2 sm:space-x-3">
            <h1 className="text-xl sm:text-2xl font-bold text-gray-900">TodoApp</h1>
          </div>

          <div className="flex items-center space-x-2 sm:space-x-4">
            <div className="hidden sm:flex items-center space-x-2 text-sm text-gray-600">
              <User className="h-4 w-4" />
              <span>{user?.username}</span>
            </div>
            {/* Mobile: Just icon */}
            <div className="flex sm:hidden items-center text-xs text-gray-600">
              <User className="h-4 w-4" />
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={logout}
              className="flex items-center space-x-1 sm:space-x-2 text-xs sm:text-sm px-2 sm:px-3"
            >
              <LogOut className="h-4 w-4" />
              <span className="hidden sm:inline">Logout</span>
            </Button>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header;
