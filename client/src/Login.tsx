import { useEffect, useState } from "react";
import { login } from "./api";
import type { PageType } from "./models/UserModel";
import { CheckCircle, XCircle } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "./authContext";



function Login() {
  const {loginAction} = useAuth();

    const navigate = useNavigate();
    const [username, setUsername] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [passwordInfo, setPasswordInfo] = useState<string[]>([]);  // Stores missing password rules
    const [isValid, setIsValid] = useState<boolean> (false);    // Controls button 'disabled' state
    const [currentPage, setCurrentPage] = useState<PageType> (1);  // 1 for Login, 2 for Register
    const [success, setSuccess] = useState<boolean> (false);  // UI state for success alerts
    const [message, setMessage] = useState<string> ('');    // API response message

    

    // Logic: Password Validation
    const passwordRequirements = ()=> {

        const hasMinLength = password.length >= 8;
        const startsWithUppercase = /^[A-Z]/.test(password);
        const hasNumber = /\d/.test(password);
        const hasSpecialChar = /[^a-zA-Z0-9]/.test(password);

        // Update 'isValid' state based on all rules passing
        setIsValid(hasMinLength && startsWithUppercase && hasNumber && hasSpecialChar);

        const failedRules = [];
        if (!hasMinLength) failedRules.push("At least 8 characters");
        if (!startsWithUppercase) failedRules.push("Start with uppercase letter");
        if (!hasNumber) failedRules.push("Include at least one number");
        if (!hasSpecialChar) failedRules.push("Has at least one special character (e.g. !@#$%^&*)");

        setPasswordInfo(failedRules);
        
    };
    
    useEffect(() => {
      if (currentPage !== 1){  // ONLY FOR registration

         passwordRequirements();
      }
       
      }, [password, currentPage, username]);
      
      //Clear the "Requirements Met" message after 2 seconds
      useEffect(() => {
        if (isValid) {
          const timer = setTimeout(() => setPasswordInfo([]), 2000);  
          return () => clearTimeout(timer);  // Cleanup timer if user types again
        }
      }, [isValid]);
      
      useEffect(() => {
        if (success) {
          const timer = setTimeout(() => {
            setMessage('');
            setSuccess(false);
          }, 3000);
          return () => clearTimeout(timer);
        }
      }, [success]);

 const handleSubmit = async() => {

    if (currentPage == 1){

        try{
        var result = await login(username, password, 1);
        console.log('Login result:', result);
        console.log('Has accessToken?', !!result.accessToken);


        if (result.accessToken) {
            setSuccess(true);
            
            setMessage(result.Message || "Login successful");

            // Sync with AuthProvider: Updates session and global user state
            loginAction(result.user, result.accessToken)
            
    
            // Redirect to home after a brief delay so user can see success message
            setTimeout(() => {
                navigate('/');
            }, 1500);

          } else {
            setSuccess(false);
            setMessage(result?.Message || "Login failed");

          }
        } catch (error) {
          setSuccess(false);
          setMessage("An error occured during login");
          console.error(error);
        }
        }
        // Handling Registration (Page 2)
        else {
        
        try {
          const registerResponse = await login(username, password, 2);
    
        setSuccess(true);

        // Log user in automatically after successful registration
        loginAction(registerResponse.user, registerResponse.accessToken);

        setMessage(registerResponse.Message || "Registration successful");
        setTimeout(() => {
            
            navigate('/');
        }, 1500);
          
        } catch (error) {
          setSuccess(false);
          setMessage("An error occurred during registration");

          console.error(error);
        }
      }
    };


    return (
        <main className="flex flex-col items-center justify-center min-h-screen bg-gray-50 px-4">
            <section className="w-full max-w-md bg-white rounded-lg shadow-md p-6 space-y-6">
            {/* API Feedback Alert */}
            {message && (success ? (
        <div className="flex items-center gap-2 p-4 rounded-md bg-green-100 border border-green-400 text-green-700 text-sm">
          <CheckCircle className="w-5 h-5 text-green-600" />
          <span>{message}</span>
        </div>
      ) : (
        <div className="flex items-center gap-2 p-4 rounded-md bg-red-100 border border-red-400 text-red-700 text-sm">
          <XCircle className="w-5 h-5 text-red-600" />
          <span>{message}</span>
        </div>
      )
      )}
      {/* Tab Switcher: Login vs Register */}
      <header className="flex justify-center space-x-4">
                    <button 
                        type="button"
                        onClick={() => setCurrentPage(1)}
                        className={`px-4 py-2 rounded-md font-semibold ${currentPage == 1 ? 'bg-gray-400 text-white' : 'bg-gray-100 text-gray-600'
                            }`}
                    >
                        Login
                    </button>
                    <button
                        type="button"
                        onClick={() => setCurrentPage(2)}
                        className={`px-4 py-2 rounded-md font-semibold ${currentPage == 2 ? 'bg-gray-400 text-white' : 'bg-gray-100 text-gray-600'
                            }`}
                    >
                        Register
                    </button>
                </header>

                {/* Form Logic */}              
                <form onSubmit={(e) => { e.preventDefault(); handleSubmit(); }} className="space-y-4">
                    <div>
                        <label htmlFor="username" className="text-sm font-medium text-gray-700">
                            Username
                        </label>
                        <input
                            id="username"
                            type="text"
                            placeholder="Enter your username"
                            className="mt-1 block w-full border border-gray-300 rounded-md p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            required
                            onChange={(e) => setUsername(e.target.value)}
                        />
                    </div>

                    <div>
                        <label htmlFor="password" className="block text-sm font-medium text-gray-700">
                            Password
                        </label>
                        <input
                            id="password"
                            type="password"
                            placeholder="Enter your password"
                            className="mt-1 block w-full border border-gray-300 rounded-md p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            required
                            onChange={(e) => { setPassword(e.target.value); }}
                        />
                    </div>
                    {/* Conditional Rendering: Password requirements (only for registration) */}
                    {currentPage== 2 &&( isValid ? <p className="text-sm text-green-500">Your password fulfills all the required criteria</p>  : (
                        <div className="text-sm text-red-500">
                            {passwordInfo.map((info, index) => (
                                <div key={index}>{info}</div>
                            ))}
                        </div>
                    ))}

                    <button
                        type="submit"
                        id="loginButton"
                        disabled = {!isValid}
                        className = {isValid ? "w-full bg-black text-white font-semibold py-2 rounded-md hover:bg-blue-700 transition-colors duration-300"
                            : "w-full bg-gray-400 text-white font-semibold py-2 rounded-md"}
                    >
                        {currentPage == 2 ? 'Register' : 'Login'}
                    </button>
                </form>
            </section>
        </main>
    );
}

export default Login;