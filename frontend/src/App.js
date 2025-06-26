import React, { useState } from 'react';
import './App.css';

function App() {
  const [guess, setGuess] = useState('');
  const [feedback, setFeedback] = useState('');
  const [history, setHistory] = useState([]);

  const handleGuess = async () => {
    if (!guess) return;
    const res = await fetch('http://localhost:5143/guess', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ number: parseInt(guess), user: "Raphael" })
    });
    const data = await res.json();
    setFeedback(data.message);
    if (data.tries !== undefined) {
      setHistory(data.history);
    }
    setGuess('');
  };

  return (
    <div className="container">
      <h1>Adivinhe o Número (1–100)</h1>
      <input
        type="number"
        value={guess}
        onChange={(e) => setGuess(e.target.value)}
        placeholder="Digite seu palpite"
      />
      <button onClick={handleGuess}>Enviar</button>
      {feedback && <p className="feedback">{feedback}</p>}
      {history.length > 0 && (
        <div>
          <h2>Ranking</h2>
          <ul>
            {history.map((entry, i) => (
              <li key={i}>{entry.user}: {entry.tries} tentativas</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

export default App;
